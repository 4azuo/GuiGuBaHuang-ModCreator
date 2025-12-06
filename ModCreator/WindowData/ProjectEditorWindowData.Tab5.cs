using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public ObservableCollection<FileItem> EventItems { get; set; } = [];
        [NotifyMethod(nameof(OnEventItemSelected))]
        public FileItem SelectedEventItem { get; set; }
        [NotifyMethod(nameof(LoadModEventContent))]
        public ModEventItem SelectedModEvent { get; set; }
        public string EventSourceContent { get; set; }
        public bool HasSelectedEventFile => SelectedModEvent != null;
        public bool IsGuiMode { get; set; } = true;
        public List<string> CacheTypes { get; set; } = [];
        public List<string> WorkOnTypes { get; set; } = [];
        public List<EventActionBase> AvailableEvents { get; set; } = ModEventHelper.LoadModEventMethodsFromAssembly();

        public void LoadModEventFiles()
        {
            if (Project == null) return;

            var modPath = Path.Combine(Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");

            if (!Directory.Exists(modPath))
                Directory.CreateDirectory(modPath);

            EventItems.Clear();
            var items = BuildEventFileTree(modPath, modPath);
            foreach (var item in items)
                EventItems.Add(item);

            if (Project.ModEvents == null)
                Project.ModEvents = [];
        }

        private List<FileItem> BuildEventFileTree(string rootPath, string currentPath, FileItem parent = null)
        {
            var items = new List<FileItem>();

            var directories = Directory.GetDirectories(currentPath).OrderBy(d => d);
            foreach (var dir in directories)
            {
                var folderItem = new FileItem
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    IsFolder = true,
                    Parent = parent
                };

                var children = BuildEventFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                    folderItem.Children.Add(child);

                items.Add(folderItem);
            }

            var files = Directory.GetFiles(currentPath, "*.cs").OrderBy(f => f);
            foreach (var file in files)
            {
                items.Add(new FileItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsFolder = false,
                    Parent = parent
                });
            }

            return items;
        }

        public void OnEventItemSelected(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedEventItem != null && !SelectedEventItem.IsFolder)
            {
                var existingEvent = Project?.ModEvents?.FirstOrDefault(e => e.FilePath == SelectedEventItem.FullPath);
                if (existingEvent != null)
                {
                    SelectedModEvent = existingEvent;
                }
                else
                {
                    SelectedModEvent = new ModEventItem { FilePath = SelectedEventItem.FullPath };
                    Project?.ModEvents?.Add(SelectedModEvent);
                }

                if (SelectedModEvent.Conditions.Count == 0 || SelectedModEvent.Conditions[0].Name != Constants.EventActionRootElement.Name)
                {
                    SelectedModEvent.Conditions.Insert(0, Constants.EventActionRootElement);
                }

                if (SelectedModEvent.Actions.Count == 0 || SelectedModEvent.Actions[0].Name != Constants.EventActionRootElement.Name)
                {
                    SelectedModEvent.Actions.Insert(0, Constants.EventActionRootElement);
                }
            }
            else
                SelectedModEvent = null;
        }

        public void LoadModEventContent(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            EventSourceContent = SelectedModEvent != null && File.Exists(SelectedModEvent.FilePath)
                ? File.ReadAllText(SelectedModEvent.FilePath)
                : string.Empty;
            
            // Update the content in the corresponding FileItem
            if (SelectedEventItem != null && !SelectedEventItem.IsFolder && SelectedModEvent != null)
            {
                SelectedEventItem.Content = EventSourceContent;
            }
        }

        public void SaveModEvent()
        {
            if (SelectedModEvent == null || string.IsNullOrEmpty(SelectedModEvent.FilePath))
                return;

            var content = IsGuiMode ? GenerateModEventCode(SelectedModEvent) : EventSourceContent;
            File.WriteAllText(SelectedModEvent.FilePath, content);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFile", Path.GetFileName(SelectedModEvent.FilePath));
        }

        public void SaveModEvents()
        {
            if (Project == null)
                return;

            // Update current item's content
            if (SelectedEventItem != null && !SelectedEventItem.IsFolder && SelectedModEvent != null)
            {
                var content = IsGuiMode ? GenerateModEventCode(SelectedModEvent) : EventSourceContent;
                SelectedEventItem.Content = content;
            }

            // Save all files in EventItems
            int savedCount = 0;
            SaveAllEventItems(EventItems, ref savedCount);
            
            if (savedCount > 0)
                StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFiles", savedCount);
        }

        private void SaveAllEventItems(ObservableCollection<FileItem> items, ref int savedCount)
        {
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    // Recursively save children
                    SaveAllEventItems(item.Children, ref savedCount);
                }
                else
                {
                    // Find the ModEventItem for this file
                    var modEvent = Project?.ModEvents?.FirstOrDefault(e => e.FilePath == item.FullPath);
                    if (modEvent != null)
                    {
                        var content = string.Empty;
                        
                        // If this is the selected item, use current GUI/code mode
                        if (item == SelectedEventItem)
                        {
                            content = IsGuiMode ? GenerateModEventCode(modEvent) : EventSourceContent;
                        }
                        else
                        {
                            // For other items, check if they have stored content or generate from ModEventItem
                            if (!string.IsNullOrEmpty(item.Content))
                            {
                                content = item.Content;
                            }
                            else if (!modEvent.IsCodeModeOnly)
                            {
                                content = GenerateModEventCode(modEvent);
                            }
                            else if (File.Exists(item.FullPath))
                            {
                                // Code mode only, keep existing file content
                                content = File.ReadAllText(item.FullPath);
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(content))
                        {
                            File.WriteAllText(item.FullPath, content);
                            savedCount++;
                        }
                    }
                }
            }
        }

        public string GenerateModEventCode(ModEventItem modEvent)
        {
            if (modEvent == null || string.IsNullOrEmpty(modEvent.FilePath))
                return string.Empty;

            // Load templates
            var eventTemplate = ResourceHelper.ReadEmbeddedResource("ModCreator.Resources.EventTemplate.tmp");
            var eventTemplateContent = ResourceHelper.ReadEmbeddedResource("ModCreator.Resources.EventTemplateContent.tmp");

            if (string.IsNullOrEmpty(eventTemplate) || string.IsNullOrEmpty(eventTemplateContent))
                return string.Empty;

            // Get class name from file name
            var className = modEvent.FileName;

            // Generate event method signature and base call
            string eventMethod;
            string baseCall = string.Empty;
            
            if (modEvent.EventMode == Enums.EventMode.ModEvent && !string.IsNullOrEmpty(modEvent.SelectedEvent))
            {
                // Find the event method signature from available events
                var selectedEvent = AvailableEvents.FirstOrDefault(e => e.Name == modEvent.SelectedEvent);
                if (selectedEvent != null)
                {
                    eventMethod = $"public override {selectedEvent.Code}";
                    
                    // Generate base call with parameters
                    if (selectedEvent.Parameters != null && selectedEvent.Parameters.Count > 0)
                    {
                        var paramNames = string.Join(", ", selectedEvent.Parameters.Select(p => p.Name));
                        baseCall = $"base.{selectedEvent.Name}({paramNames});";
                    }
                    else
                    {
                        baseCall = $"base.{selectedEvent.Name}();";
                    }
                }
                else
                {
                    eventMethod = $"public override void {modEvent.SelectedEvent}()";
                    baseCall = $"base.{modEvent.SelectedEvent}();";
                }
            }
            else
            {
                // NonEvent mode - use custom event name or default "Run"
                eventMethod = $"public void Run()";
                // No base call for non-event methods
            }

            // Generate condition code
            var conditionCode = GenerateCodeFromEventActions(modEvent.Conditions, isCondition: true);
            if (string.IsNullOrEmpty(conditionCode))
                conditionCode = "true";

            // Generate action code
            var actionCode = GenerateCodeFromEventActions(modEvent.Actions, isCondition: false);
            if (string.IsNullOrEmpty(actionCode))
                actionCode = "// No actions";

            // Replace placeholders in event content
            var eventContent = eventTemplateContent
                .Replace("#BASECALL#", baseCall)
                .Replace("#EVENTMETHOD#", eventMethod)
                .Replace("#CONDITION#", conditionCode)
                .Replace("#ACTION#", actionCode);

            // Replace placeholders in main template
            var cacheType = string.IsNullOrEmpty(modEvent.CacheType) ? "Local" : $"{modEvent.CacheType}";
            var workOn = string.IsNullOrEmpty(modEvent.WorkOn) ? "Local" : $"{modEvent.WorkOn}";

            var generatedCode = eventTemplate
                .Replace("#PROJECTID#", Project.ProjectId)
                .Replace("#CLASSNAME#", className)
                .Replace("#CACHETYPE#", cacheType)
                .Replace("#WORKON#", workOn)
                .Replace("#ORDERINDEX#", modEvent.OrderIndex.ToString())
                .Replace("#EVENTCONTENT#", eventContent);

            return generatedCode;
        }

        private string GenerateCodeFromEventActions(ObservableCollection<EventActionBase> actions, bool isCondition = false)
        {
            if (actions == null || actions.Count == 0)
                return string.Empty;

            var codeBuilder = new System.Text.StringBuilder();
            var indent = isCondition ? "" : "                "; // 16 spaces for actions, no indent for conditions
            var needsSeparator = false;

            foreach (var action in actions)
            {
                // Skip the Root placeholder - but process its children
                if (action.Name == Constants.EventActionRootElement.Name)
                {
                    if (action.Children != null && action.Children.Count > 0)
                    {
                        foreach (var child in action.Children)
                        {
                            var code = GenerateCodeFromSingleAction(child, isCondition);
                            if (!string.IsNullOrEmpty(code))
                            {
                                var trimmedCode = code.TrimEnd();
                                
                                if (isCondition)
                                {
                                    // For conditions, wrap each in () and join with space
                                    if (needsSeparator)
                                        codeBuilder.Append(" ");
                                    codeBuilder.Append($"({trimmedCode})");
                                    needsSeparator = true;
                                }
                                else
                                {
                                    // For actions, add indent and semicolon
                                    if (!trimmedCode.EndsWith(";") && !trimmedCode.EndsWith("}"))
                                    {
                                        codeBuilder.AppendLine($"{indent}{trimmedCode};");
                                    }
                                    else
                                    {
                                        codeBuilder.AppendLine($"{indent}{trimmedCode}");
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }

                var actionCode = GenerateCodeFromSingleAction(action, isCondition);
                if (!string.IsNullOrEmpty(actionCode))
                {
                    var trimmedCode = actionCode.TrimEnd();
                    
                    if (isCondition)
                    {
                        // For conditions, wrap each in () and join with space
                        if (needsSeparator)
                            codeBuilder.Append(" ");
                        codeBuilder.Append($"({trimmedCode})");
                        needsSeparator = true;
                    }
                    else
                    {
                        // For actions, add indent and semicolon
                        if (!trimmedCode.EndsWith(";") && !trimmedCode.EndsWith("}"))
                        {
                            codeBuilder.AppendLine($"{indent}{trimmedCode};");
                        }
                        else
                        {
                            codeBuilder.AppendLine($"{indent}{trimmedCode}");
                        }
                    }
                }
            }

            return codeBuilder.ToString().TrimEnd();
        }

        private string GenerateCodeFromSingleAction(EventActionBase action, bool isCondition = false)
        {
            if (action == null || string.IsNullOrEmpty(action.Code))
                return string.Empty;

            var code = isCondition ? $"({action.Code})" : action.Code;

            // Replace parameter placeholders with actual values
            if (action.Parameters != null && action.Parameters.Count > 0)
            {
                for (int i = 0; i < action.Parameters.Count; i++)
                {
                    var placeholder = $"{{{i}}}";
                    
                    // Check if this parameter has a value
                    if (action.ParameterValues != null && action.ParameterValues.ContainsKey(i))
                    {
                        var paramValue = action.ParameterValues[i];
                        if (paramValue != null)
                        {
                            var paramCode = GenerateCodeFromParameterValue(paramValue, isCondition);
                            code = code.Replace(placeholder, paramCode);
                        }
                        else
                        {
                            // Parameter is null, use empty or default
                            code = code.Replace(placeholder, "/* missing parameter */");
                        }
                    }
                    else
                    {
                        // Parameter not provided, use parameter name or placeholder comment
                        var paramName = action.Parameters[i].Name;
                        code = code.Replace(placeholder, $"/* {paramName} */");
                    }
                }
            }

            // Process children recursively if HasBody is true
            if (action.HasBody && action.Children != null && action.Children.Count > 0)
            {
                var childrenCode = new System.Text.StringBuilder();
                foreach (var child in action.Children)
                {
                    var childCode = GenerateCodeFromSingleAction(child, isCondition);
                    if (!string.IsNullOrEmpty(childCode))
                    {
                        childrenCode.AppendLine($"    {childCode};");
                    }
                }
                
                if (childrenCode.Length > 0)
                {
                    code = code.Replace("{BODY}", childrenCode.ToString().TrimEnd());
                }
            }

            return code;
        }

        private string GenerateCodeFromParameterValue(ModEventItemSelectValue paramValue, bool isCondition = false)
        {
            if (paramValue == null)
                return string.Empty;

            switch (paramValue.SelectType)
            {
                case Enums.ModEventSelectType.EventAction:
                    // Generate code from nested EventAction
                    if (paramValue.SelectedEventAction != null)
                    {
                        var nestedCode = GenerateCodeFromSingleAction(paramValue.SelectedEventAction, isCondition);
                        // For nested actions used as parameters, remove trailing semicolon
                        return nestedCode?.TrimEnd(';', ' ', '\r', '\n') ?? string.Empty;
                    }
                    return string.Empty;

                case Enums.ModEventSelectType.Variable:
                    return paramValue.SelectedVariable?.Name ?? string.Empty;

                case Enums.ModEventSelectType.OptionalValue:
                    // Return the optional value as-is (could be code snippet)
                    return paramValue.OptionalValue ?? string.Empty;

                default:
                    return string.Empty;
            }
        }
    }
}
