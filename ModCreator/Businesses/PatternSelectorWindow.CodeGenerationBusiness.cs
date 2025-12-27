using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using ModCreator.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for PatternSelectorWindow code generation
    /// Handles validation, JSON generation, and file saving for patterns
    /// </summary>
    public class PatternSelectorWindowCodeGenerationBusiness
    {
        private readonly PatternSelectorWindowData _windowData;
        private readonly Window _owner;

        public PatternSelectorWindowCodeGenerationBusiness(PatternSelectorWindowData windowData, Window owner)
        {
            _windowData = windowData ?? throw new System.ArgumentNullException(nameof(windowData));
            _owner = owner ?? throw new System.ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Validates and saves pattern data to JSON files
        /// </summary>
        /// <param name="projectPath">Project path</param>
        /// <returns>True if save successful</returns>
        public bool SavePatternFiles(string projectPath)
        {
            if (_windowData.SelectedPattern == null)
            {
                MessageBox.Show(
                    MessageHelper.Get("Messages.Warning.NoPatternSelected"),
                    MessageHelper.Get("Messages.Warning.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(projectPath))
            {
                MessageBox.Show(
                    MessageHelper.Get("Messages.Error.NoProject"),
                    MessageHelper.Get("Messages.Error.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (!ValidateData(out var validationErrors))
            {
                NotificationWindow.ShowDetails(
                    _owner,
                    MessageHelper.Get("Messages.Error.Title"),
                    "Validation errors found:",
                    validationErrors,
                    NotificationType.Error);
                return false;
            }

            var confPath = Path.Combine(projectPath, "ModProject", "ModConf");
            Directory.CreateDirectory(confPath);

            SaveFiles(confPath);

            MessageBox.Show(
                MessageHelper.GetFormat("Messages.Success.PatternSaved", _windowData.SelectedPattern.Name),
                MessageHelper.Get("Messages.Success.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return true;
        }

        /// <summary>
        /// Validates all pattern data
        /// </summary>
        private bool ValidateData(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            foreach (var file in _windowData.DisplayFiles)
            {
                ValidateUniqueValues(file, validationErrors);
                ValidateRequiredAndTypes(file, validationErrors);
            }

            return validationErrors.Count == 0;
        }

        /// <summary>
        /// Validates unique values in pattern elements
        /// </summary>
        private void ValidateUniqueValues(PatternFileDisplay file, List<string> validationErrors)
        {
            var uniqueElements = file.Elements.Where(e => e.Unique).ToList();

            foreach (var uniqueElement in uniqueElements)
            {
                var values = new Dictionary<string, int>();
                var rowIndex = 0;

                foreach (var row in file.Rows)
                {
                    rowIndex++;
                    if (row.RowData.ContainsKey(uniqueElement.Name))
                    {
                        var value = row.RowData[uniqueElement.Name];
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if (values.ContainsKey(value))
                            {
                                validationErrors.Add(
                                    $"{file.FileName} - Row {rowIndex}: {uniqueElement.Label} '{value}' is duplicated (first appeared in row {values[value]})");
                            }
                            else
                            {
                                values[value] = rowIndex;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates required fields and data types
        /// </summary>
        private void ValidateRequiredAndTypes(PatternFileDisplay file, List<string> validationErrors)
        {
            foreach (var row in file.Rows)
            {
                foreach (var element in file.Elements)
                {
                    var value = row.RowData.ContainsKey(element.Name) ? row.RowData[element.Name] : string.Empty;

                    if (element.Required && string.IsNullOrWhiteSpace(value))
                    {
                        validationErrors.Add($"{file.FileName}: {element.Label} is required");
                    }
                    else if (!string.IsNullOrWhiteSpace(value) && !element.ValidateValue(value, element.VarType))
                    {
                        validationErrors.Add($"{file.FileName}: {element.Label} - {element.ValidationError}");
                    }
                }
            }
        }

        /// <summary>
        /// Saves all pattern files to JSON
        /// </summary>
        private void SaveFiles(string confPath)
        {
            foreach (var file in _windowData.DisplayFiles)
            {
                var fileName = GetFileName(file.FileName);
                var filePath = Path.Combine(confPath, fileName);
                var jsonArray = BuildJsonArray(file);

                if (jsonArray.Count > 0)
                {
                    var jsonContent = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                    FileHelper.WriteTextFile(filePath, jsonContent);
                }
            }
        }

        /// <summary>
        /// Gets filename with prefix if specified
        /// </summary>
        private string GetFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(_windowData.Prefix))
                return fileName;

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            return $"{_windowData.Prefix}_{fileNameWithoutExt}{ext}";
        }

        /// <summary>
        /// Builds JSON array from pattern file display
        /// </summary>
        private List<Dictionary<string, object>> BuildJsonArray(PatternFileDisplay file)
        {
            var jsonArray = new List<Dictionary<string, object>>();

            foreach (var row in file.Rows)
            {
                var jsonObject = BuildJsonObject(file.Elements, row);
                if (jsonObject.Count > 0)
                    jsonArray.Add(jsonObject);
            }

            return jsonArray;
        }

        /// <summary>
        /// Builds JSON object from row data
        /// </summary>
        private Dictionary<string, object> BuildJsonObject(ObservableCollection<PatternElement> elements, RowDisplay row)
        {
            var jsonObject = new Dictionary<string, object>();

            foreach (var element in elements)
            {
                if (element.ParentElement != null)
                    continue;

                var value = GetElementValue(element, row.RowData);
                if (!string.IsNullOrWhiteSpace(value))
                    jsonObject[element.Name] = value;
            }

            return jsonObject;
        }

        /// <summary>
        /// Gets element value from row data
        /// </summary>
        private string GetElementValue(PatternElement element, Dictionary<string, string> rowData)
        {
            if (element.Type == "composite")
                return PatternHelper.ProcessCompositeValue(element, rowData);

            if (element.IsAutoGenerated)
                return PatternHelper.ProcessAutoGenValue(element.ElementFormat, rowData);

            return rowData.ContainsKey(element.Name) ? rowData[element.Name] : string.Empty;
        }
    }
}
