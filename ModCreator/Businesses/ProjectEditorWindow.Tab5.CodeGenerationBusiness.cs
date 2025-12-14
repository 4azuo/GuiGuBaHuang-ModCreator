using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for Tab5 ModEvent code generation
    /// Handles generating ModEvent code files
    /// </summary>
    public class Tab5CodeGenerationBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private readonly Window _owner;

        public Tab5CodeGenerationBusiness(ProjectEditorWindowData windowData, Window owner)
        {
            _windowData = windowData ?? throw new System.ArgumentNullException(nameof(windowData));
            _owner = owner ?? throw new System.ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Generates code for ModEvent and saves to file
        /// </summary>
        public string GenerateModEventCode()
        {
            if (_windowData.SelectedModEvent == null)
            {
                MessageBox.Show(
                    MessageHelper.Get("Messages.Warning.NoModEventSelected"),
                    MessageHelper.Get("Messages.Warning.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return string.Empty;
            }

            var outputPath = SaveModEvent(_windowData.SelectedModEvent);
            
            if (!string.IsNullOrEmpty(outputPath))
            {
                ShowCodeGeneratedMessage(outputPath);
            }

            return outputPath;
        }

        /// <summary>
        /// Saves ModEvent to file
        /// </summary>
        private string SaveModEvent(ModEventItem modEventItem, bool showStatusMsg = true)
        {
            if (modEventItem == null || string.IsNullOrEmpty(modEventItem.FilePath))
                return string.Empty;

            var content = !modEventItem.IsCodeModeOnly ? GenerateCode(modEventItem) : string.Empty;
            
            if (!string.IsNullOrEmpty(content))
            {
                File.WriteAllText(modEventItem.FilePath, content);
            }
            
            if (showStatusMsg)
                _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFile", Path.GetFileName(modEventItem.FilePath));

            return modEventItem.FilePath;
        }

        /// <summary>
        /// Generates ModEvent code from template
        /// </summary>
        private string GenerateCode(ModEventItem modEvent)
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
            
            if (modEvent.EventMode == EventMode.ModEvent && !string.IsNullOrEmpty(modEvent.SelectedEvent))
            {
                // Find the event method signature from available events
                var selectedEvent = _windowData.AvailableEvents.FirstOrDefault(e => e.Name == modEvent.SelectedEvent);
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
                .Replace("#PROJECTID#", _windowData.Project.ProjectId)
                .Replace("#CLASSNAME#", className)
                .Replace("#CACHETYPE#", cacheType)
                .Replace("#WORKON#", workOn)
                .Replace("#ORDERINDEX#", modEvent.OrderIndex.ToString())
                .Replace("#EVENTCONTENT#", eventContent);

            return generatedCode;
        }

        private string GenerateCodeFromEventActions(ObservableCollection<EventActionBase> actions, bool isCondition)
        {
            var codeLines = GenerateCodeFromEventActions(actions);
            if (codeLines == null || codeLines.Count == 0)
                return string.Empty;

            if (isCondition)
            {
                return string.Join(" ", codeLines.Select(x => $"({x})"));
            }
            else
            {
                // Add proper indentation to each line based on brace depth
                var indentedLines = new List<string>();
                int indentLevel = 0;
                foreach (var line in codeLines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Decrease indent before closing brace
                    if (trimmedLine == "}")
                        indentLevel--;
                    
                    // Add indentation
                    var indent = new string(' ', indentLevel * 4);
                    indentedLines.Add($"                {indent}{trimmedLine}");
                    
                    // Increase indent after opening brace
                    if (trimmedLine == "{")
                        indentLevel++;
                }
                
                return string.Join("\r\n", indentedLines);
            }
        }

        private List<string> GenerateCodeFromEventActions(ObservableCollection<EventActionBase> actions)
        {
            if (actions == null || actions.Count == 0)
                return [];

            var codeLines = new List<string>();
            foreach (var action in actions)
            {
                // Skip the Root placeholder - but process its children
                if (action.Name == Constants.EventActionRootElement.Name)
                {
                    foreach (var child in action.Children)
                    {
                        codeLines.AddRange(GenerateCodeFromSingleAction(child));
                    }
                    break;
                }
            }

            return codeLines;
        }

        private List<string> GenerateCodeFromSingleAction(EventActionBase action)
        {
            if (action == null || string.IsNullOrEmpty(action.Code))
                return [];

            var codeLines = new List<string>();
            if (!string.IsNullOrEmpty(action.Code))
                codeLines.Add(GenerateCodeWithParameters(action));
            foreach (var child in action.Children)
            {
                if (child.IsCanAddChild)
                {
                    if (!string.IsNullOrEmpty(child.Code))
                        codeLines.Add(GenerateCodeWithParameters(child));
                    codeLines.Add("{");
                    foreach (var c in child.Children)
                    {
                        codeLines.AddRange(GenerateCodeFromSingleAction(c));
                    }
                    codeLines.Add("}");
                }
                else
                {
                    codeLines.AddRange(GenerateCodeFromSingleAction(child));
                }
            }

            return codeLines;
        }

        private string GenerateCodeWithParameters(EventActionBase action)
        {
            var code = action.Code;
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
                            var paramCode = GenerateCodeFromParameterValue(paramValue);
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
            return code;
        }

        private string GenerateCodeFromParameterValue(ModEventItemSelectValue paramValue)
        {
            if (paramValue == null)
                return string.Empty;

            switch (paramValue.SelectType)
            {
                case ModEventSelectType.EventAction:
                    // Generate code from nested EventAction
                    if (paramValue.SelectedEventAction != null)
                    {
                        var nestedCodeLines = GenerateCodeFromSingleAction(paramValue.SelectedEventAction);
                        if (nestedCodeLines != null && nestedCodeLines.Count > 0)
                        {
                            // Join all code lines and trim trailing semicolon for inline usage
                            var nestedCode = string.Join(" ", nestedCodeLines);
                            return nestedCode.TrimEnd(';', ' ', '\r', '\n');
                        }
                    }
                    return string.Empty;

                case ModEventSelectType.Variable:
                    return paramValue.SelectedVariable?.Name ?? string.Empty;

                case ModEventSelectType.OptionalValue:
                    // Return the optional value as-is (could be code snippet)
                    return paramValue.OptionalValue ?? string.Empty;

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Shows success message after generating ModEvent code
        /// </summary>
        private void ShowCodeGeneratedMessage(string outputPath)
        {
            MessageBox.Show(
                MessageHelper.GetFormat("Messages.Success.ModEventCodeGenerated", outputPath),
                MessageHelper.Get("Messages.Success.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
