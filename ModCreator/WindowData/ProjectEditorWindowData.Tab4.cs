using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public ObservableCollection<GlobalVariable> GlobalVariables { get; set; } = [];
        public List<VarType> VarTypes { get; set; } = ValidatedModel.VarTypes;

        public void LoadGlobalVariables()
        {
            GlobalVariables = new ObservableCollection<GlobalVariable>(Project.GlobalVariables);
        }

        public string GenerateVariables()
        {

            if (!GlobalVariables.Any())
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.NoVariablesToGenerate"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return string.Empty;
            }

            // Validate all variables before generating code
            var validationErrors = new List<string>();

            foreach (var variable in GlobalVariables)
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
            var duplicateGroups = GlobalVariables
                .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                .GroupBy(v => v.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var dupName in duplicateGroups)
            {
                validationErrors.Add($"• Duplicate variable name: '{dupName}'");
            }

            // Show all validation errors
            if (validationErrors.Any())
            {
                var errorMessage = "Cannot generate code due to validation errors:\n\n" + string.Join("\n", validationErrors);
                MessageBox.Show(errorMessage, MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }

            var varTemplate = ResourceHelper.ReadEmbeddedResource("ModCreator.Resources.VarTemplate.tmp");
            var varTemplateContent = ResourceHelper.ReadEmbeddedResource("ModCreator.Resources.VarTemplateContent.tmp");

            if (string.IsNullOrEmpty(varTemplate) || string.IsNullOrEmpty(varTemplateContent))
                return string.Empty;

            var variableProperties = new System.Text.StringBuilder();
            foreach (var variable in GlobalVariables)
            {
                if (string.IsNullOrWhiteSpace(variable.Name)) continue;

                var propertyCode = varTemplateContent
                    .Replace("#VARTYPE#", variable.Type ?? "String")
                    .Replace("#VARNAME#", variable.Name)
                    .Replace("#VARVALUE#", FormatVariableValue(variable));

                variableProperties.AppendLine($"        {propertyCode.Trim()} // {variable.Description}");
            }

            var generatedCode = varTemplate.Replace("#VARIABLES#", variableProperties.ToString());
            var outputPath = Path.Combine(Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllText(outputPath, generatedCode);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.GeneratedVariablesCode", GlobalVariables.Count);

            return outputPath;
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

        public string SaveGlobalVariables()
        {
            if (Project == null) return string.Empty;
            var outputPath = GenerateVariables();
            Project.GlobalVariables = new List<GlobalVariable>(GlobalVariables);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedGlobalVariables", GlobalVariables.Count);
            return outputPath;
        }
    }
}