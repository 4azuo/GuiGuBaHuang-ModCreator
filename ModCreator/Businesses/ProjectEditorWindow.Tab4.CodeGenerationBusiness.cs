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
    /// Business logic for Tab4 global variables code generation
    /// Handles generating code for Global Variables (Tab4)
    /// </summary>
    public class Tab4CodeGenerationBusiness
    {
        private readonly ProjectEditorWindowData _windowData;

        public Tab4CodeGenerationBusiness(ProjectEditorWindowData windowData)
        {
            _windowData = windowData ?? throw new System.ArgumentNullException(nameof(windowData));
        }

        /// <summary>
        /// Generates code for Global Variables and saves to file
        /// </summary>
        /// <returns>Output file path</returns>
        public string GenerateGlobalVariablesCode()
        {
            if (_windowData.GlobalVariablesContainer == null || !_windowData.GlobalVariablesContainer.Variables.Any())
            {
                MessageBox.Show(
                    MessageHelper.Get("Messages.Error.NoVariablesToGenerate"),
                    MessageHelper.Get("Messages.Warning.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return string.Empty;
            }

            // Validate all variables before generating code
            if (!ValidateVariables(out var validationErrors))
            {
                var errorMessage = "Cannot generate code due to validation errors:\n\n" + string.Join("\n", validationErrors);
                MessageBox.Show(
                    errorMessage,
                    MessageHelper.Get("Messages.Error.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return string.Empty;
            }

            var varTemplate = ResourceHelper.ReadEmbeddedResource("ModCreator.Resources.VarTemplate.tmp");
            var varTemplateContent = ResourceHelper.ReadEmbeddedResource("ModCreator.Resources.VarTemplateContent.tmp");

            if (string.IsNullOrEmpty(varTemplate) || string.IsNullOrEmpty(varTemplateContent))
                return string.Empty;

            var variableProperties = new System.Text.StringBuilder();
            foreach (var variable in _windowData.GlobalVariablesContainer.Variables)
            {
                if (string.IsNullOrWhiteSpace(variable.Name)) continue;

                var propertyCode = varTemplateContent
                    .Replace("#VARTYPE#", variable.Type ?? "String")
                    .Replace("#VARNAME#", variable.Name)
                    .Replace("#VARVALUE#", FormatVariableValue(variable));

                variableProperties.AppendLine($"        {propertyCode.Trim()} // {variable.Description}");
            }

            var generatedCode = varTemplate
                .Replace("#PROJECTID#", _windowData.Project.ProjectId)
                .Replace("#VARIABLES#", variableProperties.ToString());

            var outputPath = Path.Combine(
                _windowData.Project.ProjectPath,
                "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllText(outputPath, generatedCode);

            // Save from container back to Project
            _windowData.Project.GlobalVariables = new ObservableCollection<GlobalVariable>(_windowData.GlobalVariablesContainer.Variables);

            _windowData.StatusMessage = MessageHelper.GetFormat(
                "Messages.Success.GeneratedVariablesCode",
                _windowData.GlobalVariablesContainer.Variables.Count);

            return outputPath;
        }

        /// <summary>
        /// Validates all variables before code generation
        /// </summary>
        private bool ValidateVariables(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            foreach (var variable in _windowData.GlobalVariablesContainer.Variables)
            {
                // Check for empty name
                if (string.IsNullOrWhiteSpace(variable.Name))
                {
                    validationErrors.Add("• Variable with empty name found");
                    continue;
                }

                // Validate name format
                if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(variable.Name))
                {
                    validationErrors.Add($"• Invalid variable name: '{variable.Name}' (not a valid C# identifier)");
                }

                // Check for empty type
                if (string.IsNullOrWhiteSpace(variable.Type))
                {
                    validationErrors.Add($"• Variable '{variable.Name}' has no type specified");
                }

                // Validate value against type
                if (!string.IsNullOrWhiteSpace(variable.Value) && !string.IsNullOrWhiteSpace(variable.Type))
                {
                    if (!variable.ValidateValue(variable.Value, variable.Type))
                    {
                        validationErrors.Add($"• Variable '{variable.Name}': {variable.ValidationError}");
                    }
                }
            }

            // Check for duplicate names
            var duplicateGroups = _windowData.GlobalVariablesContainer.Variables
                .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                .GroupBy(v => v.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var dupName in duplicateGroups)
            {
                validationErrors.Add($"• Duplicate variable name: '{dupName}'");
            }

            return validationErrors.Count == 0;
        }

        /// <summary>
        /// Format variable value for code generation
        /// </summary>
        private string FormatVariableValue(GlobalVariable variable)
        {
            var varType = variable.Type?.ToLower();

            // Default values when Value is empty
            var defaultValues = new Dictionary<string, string>
            {
                ["String"] = "\"\"",
                ["Boolean"] = "false",
                ["Int32"] = "0",
                ["Int64"] = "0L",
                ["Single"] = "0f",
                ["Double"] = "0.0"
            };

            if (string.IsNullOrWhiteSpace(variable.Value))
            {
                if (defaultValues.TryGetValue(varType ?? "", out var defaultValue))
                    return defaultValue;
                return "null"; // Arrays and unknown types default to null
            }

            var value = variable.Value.Trim();

            // Format value based on type
            return varType switch
            {
                "String" => value.StartsWith("\"") && value.EndsWith("\"") ? value : $"\"{value}\"",
                "Boolean" => value.ToLower() is "true" or "false" ? value.ToLower() : "false",
                "Single" => value.EndsWith("f") || value.EndsWith("F") ? value : $"{value}f",
                "Int64" => value.EndsWith("L") || value.EndsWith("l") ? value : $"{value}L",
                _ => value
            };
        }

        /// <summary>
        /// Shows success message after generating Global Variables code
        /// </summary>
        public void ShowGlobalVariablesCodeGeneratedMessage(string outputPath)
        {
            if (string.IsNullOrEmpty(outputPath)) return;

            MessageBox.Show(
                MessageHelper.GetFormat("Messages.Success.VariablesCodeGenerated", outputPath),
                MessageHelper.Get("Messages.Info.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
