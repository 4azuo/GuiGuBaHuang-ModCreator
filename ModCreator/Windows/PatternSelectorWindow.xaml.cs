using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class PatternSelectorWindow : CWindow<PatternSelectorWindowData>
    {
        public override PatternSelectorWindowData InitData(System.ComponentModel.CancelEventArgs e)
        {
            var data = base.InitData(e);
            Loaded += (s, ev) =>
            {
                var projectEditorWindow = Owner as ProjectEditorWindow;
                if (projectEditorWindow.WindowData.Project != null)
                {
                    data.SetProjectPath(projectEditorWindow.WindowData.Project.ProjectPath);
                }
            };
            return data;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedPattern == null)
            {
                MessageBox.Show(MessageHelper.Get("Messages.Warning.NoPatternSelected"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var projectEditorWindow = Owner as ProjectEditorWindow;
            if (projectEditorWindow?.WindowData?.Project == null)
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.NoProject"), MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ValidateData(out var validationErrors))
            {
                MessageBox.Show(string.Join(Environment.NewLine, validationErrors), MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confPath = Path.Combine(projectEditorWindow.WindowData.Project.ProjectPath, "ModProject", "ModConf");
            Directory.CreateDirectory(confPath);

            SaveFiles(confPath);

            projectEditorWindow.WindowData.LoadConfFiles();
            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.PatternSaved", WindowData.SelectedPattern.Name), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool ValidateData(out List<string> validationErrors)
        {
            validationErrors = new List<string>();
            foreach (var file in WindowData.DisplayFiles)
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
            return validationErrors.Count == 0;
        }

        private void SaveFiles(string confPath)
        {
            foreach (var file in WindowData.DisplayFiles)
            {
                var fileName = GetFileName(file.FileName);
                var filePath = Path.Combine(confPath, fileName);
                var jsonArray = BuildJsonArray(file);
                
                if (jsonArray.Count > 0)
                {
                    var jsonContent = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                    Helpers.FileHelper.WriteTextFile(filePath, jsonContent);
                }
            }
        }

        private string GetFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(WindowData.Prefix))
                return fileName;

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            return $"{WindowData.Prefix}_{fileNameWithoutExt}{ext}";
        }

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

        private Dictionary<string, object> BuildJsonObject(List<PatternElement> elements, RowDisplay row)
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

        private string GetElementValue(PatternElement element, Dictionary<string, string> rowData)
        {
            if (element.Type == "composite")
                return PatternHelper.ProcessCompositeValue(element, rowData);
            
            if (element.IsAutoGenerated)
                return PatternHelper.ProcessAutoGenValue(element.ElementFormat, rowData);
            
            return rowData.ContainsKey(element.Name) ? rowData[element.Name] : string.Empty;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is PatternFileDisplay file)
            {
                file.AddRow();
            }
        }

        private void RemoveRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is RowDisplay row)
            {
                var file = WindowData.DisplayFiles.FirstOrDefault(f => f.Rows.Contains(row));
                file?.RemoveRow(row);
            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.Tag is RowElementBinding binding)
            {
                WindowData.LoadDynamicOptionsForElement(binding.Element);
            }
        }
    }
}
