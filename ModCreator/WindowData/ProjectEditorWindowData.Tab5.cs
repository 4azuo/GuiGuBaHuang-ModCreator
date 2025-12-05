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

                if (SelectedModEvent.Conditions.Count == 0 || SelectedModEvent.Conditions[0].Name != "Root")
                {
                    SelectedModEvent.Conditions.Insert(0, new EventActionBase
                    {
                        Name = "Root",
                        DisplayName = "Root",
                        Code = "Root"
                    });
                }

                if (SelectedModEvent.Actions.Count == 0 || SelectedModEvent.Actions[0].Name != "Root")
                {
                    SelectedModEvent.Actions.Insert(0, new EventActionBase
                    {
                        Name = "Root",
                        DisplayName = "Root",
                        Code = "Root"
                    });
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
            var eventTemplatePath = Path.Combine(Project.ProjectPath, "EventTemplate.tmp");
            var eventTemplateContentPath = Path.Combine(Project.ProjectPath, "EventTemplateContent.tmp");

            if (!File.Exists(eventTemplatePath) || !File.Exists(eventTemplateContentPath))
                return string.Empty;

            var eventTemplate = File.ReadAllText(eventTemplatePath);
            var eventTemplateContent = File.ReadAllText(eventTemplateContentPath);

            // Get class name from file name
            var className = modEvent.FileName;

            // Generate event method signature
            string eventMethod;
            if (modEvent.EventMode == Enums.EventMode.ModEvent && !string.IsNullOrEmpty(modEvent.SelectedEvent))
            {
                // Find the event method signature from available events
                var selectedEvent = AvailableEvents.FirstOrDefault(e => e.Name == modEvent.SelectedEvent);
                if (selectedEvent != null)
                {
                    eventMethod = $"public override {selectedEvent.Code}";
                }
                else
                {
                    eventMethod = $"public override void {modEvent.SelectedEvent}()";
                }
            }
            else
            {
                // NonEvent mode - use custom event name or default "Run"
                var methodName = string.IsNullOrEmpty(modEvent.CustomEventName) ? "Run" : modEvent.CustomEventName;
                eventMethod = $"public void {methodName}()";
            }

            // Generate condition code
            var conditionCode = GenerateCodeFromEventActions(modEvent.Conditions);
            if (string.IsNullOrEmpty(conditionCode))
                conditionCode = "true";

            // Generate action code
            var actionCode = GenerateCodeFromEventActions(modEvent.Actions);
            if (string.IsNullOrEmpty(actionCode))
                actionCode = "// No actions";

            // Replace placeholders in event content
            var eventContent = eventTemplateContent
                .Replace("#EVENTMETHOD#", eventMethod)
                .Replace("#CONDITION#", conditionCode)
                .Replace("#ACTION#", actionCode);

            // Replace placeholders in main template
            var cacheType = string.IsNullOrEmpty(modEvent.CacheType) ? "CacheAttribute.CType.Local" : $"CacheAttribute.CType.{modEvent.CacheType}";
            var workOn = string.IsNullOrEmpty(modEvent.WorkOn) ? "CacheAttribute.WType.All" : $"CacheAttribute.WType.{modEvent.WorkOn}";

            var generatedCode = eventTemplate
                .Replace("#PROJECTID#", Project.ProjectID)
                .Replace("#CLASSNAME#", className)
                .Replace("#CACHETYPE#", cacheType)
                .Replace("#WORKON#", workOn)
                .Replace("#ORDERINDEX#", modEvent.OrderIndex.ToString())
                .Replace("#EVENTCONTENT#", eventContent);

            return generatedCode;
        }

        private string GenerateCodeFromEventActions(ObservableCollection<EventActionBase> actions)
        {
            if (actions == null || actions.Count == 0)
                return string.Empty;

            var codeBuilder = new System.Text.StringBuilder();
            var indent = "            "; // 3 levels of indentation

            foreach (var action in actions)
            {
                // Skip the Root placeholder
                if (action.Name == "Root")
                    continue;

                var code = GenerateCodeFromSingleAction(action);
                if (!string.IsNullOrEmpty(code))
                {
                    codeBuilder.AppendLine($"{indent}{code};");
                }
            }

            return codeBuilder.ToString().TrimEnd();
        }

        private string GenerateCodeFromSingleAction(EventActionBase action)
        {
            if (action == null || string.IsNullOrEmpty(action.Code))
                return string.Empty;

            var code = action.Code;

            // Replace parameter placeholders with actual values
            if (action.ParameterValues != null && action.ParameterValues.Count > 0)
            {
                foreach (var paramKvp in action.ParameterValues)
                {
                    var paramIndex = paramKvp.Key;
                    var paramValue = paramKvp.Value;
                    var placeholder = $"{{{paramIndex}}}";

                    if (paramValue != null)
                    {
                        var paramCode = GenerateCodeFromParameterValue(paramValue);
                        code = code.Replace(placeholder, paramCode);
                    }
                }
            }

            return code;
        }

        private string GenerateCodeFromParameterValue(ModEventItemSelectValue paramValue)
        {
            if (paramValue == null)
                return string.Empty;

            switch (paramValue.SelectType)
            {
                case Enums.ModEventSelectType.EventAction:
                    return GenerateCodeFromSingleAction(paramValue.SelectedEventAction);

                case Enums.ModEventSelectType.Variable:
                    return paramValue.SelectedVariable?.Name ?? string.Empty;

                case Enums.ModEventSelectType.OptionalValue:
                    return paramValue.OptionalValue ?? string.Empty;

                default:
                    return string.Empty;
            }
        }
    }
}
