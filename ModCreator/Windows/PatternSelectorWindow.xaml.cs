using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        private void Create_Click(object sender, RoutedEventArgs e)
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

            var validationErrors = new List<string>();
            foreach (var file in WindowData.DisplayFiles)
            {
                foreach (var element in file.Elements)
                {
                    if (element.Required && string.IsNullOrWhiteSpace(element.Value))
                    {
                        validationErrors.Add($"{file.FileName}: {element.Label} is required");
                    }
                    else if (!string.IsNullOrWhiteSpace(element.Value) && !element.ValidateValue(element.Value, element.VarType))
                    {
                        validationErrors.Add($"{file.FileName}: {element.Label} - {element.ValidationError}");
                    }
                }
            }

            if (validationErrors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, validationErrors), MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confPath = Path.Combine(projectEditorWindow.WindowData.Project.ProjectPath, "ModProject", "ModConf");
            Directory.CreateDirectory(confPath);

            foreach (var file in WindowData.DisplayFiles)
            {
                var fileName = file.FileName;
                if (!string.IsNullOrWhiteSpace(WindowData.Prefix))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    fileName = $"{WindowData.Prefix}_{fileNameWithoutExt}{ext}";
                }

                var filePath = Path.Combine(confPath, fileName);
                
                var jsonObject = new Dictionary<string, object>();
                foreach (var element in file.Elements)
                {
                    if (!string.IsNullOrWhiteSpace(element.Value))
                        jsonObject[element.Name] = element.Value;
                }

                var jsonContent = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                FileHelper.WriteTextFile(filePath, jsonContent);
            }

            projectEditorWindow.WindowData.LoadConfFiles();
            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.PatternCreated", WindowData.SelectedPattern.Name), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
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

            var validationErrors = new List<string>();
            foreach (var file in WindowData.DisplayFiles)
            {
                foreach (var element in file.Elements)
                {
                    if (element.Required && string.IsNullOrWhiteSpace(element.Value))
                    {
                        validationErrors.Add($"{file.FileName}: {element.Label} is required");
                    }
                    else if (!string.IsNullOrWhiteSpace(element.Value) && !element.ValidateValue(element.Value, element.VarType))
                    {
                        validationErrors.Add($"{file.FileName}: {element.Label} - {element.ValidationError}");
                    }
                }
            }

            if (validationErrors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, validationErrors), MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confPath = Path.Combine(projectEditorWindow.WindowData.Project.ProjectPath, "ModProject", "ModConf");

            foreach (var file in WindowData.DisplayFiles)
            {
                var fileName = file.FileName;
                if (!string.IsNullOrWhiteSpace(WindowData.Prefix))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    fileName = $"{WindowData.Prefix}_{fileNameWithoutExt}{ext}";
                }

                var filePath = Path.Combine(confPath, fileName);
                
                if (!File.Exists(filePath))
                    continue;

                var jsonObject = new Dictionary<string, object>();
                foreach (var element in file.Elements)
                {
                    if (!string.IsNullOrWhiteSpace(element.Value))
                        jsonObject[element.Name] = element.Value;
                }

                var jsonContent = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                FileHelper.WriteTextFile(filePath, jsonContent);
            }

            projectEditorWindow.WindowData.LoadConfFiles();
            MessageBox.Show("Files updated successfully", MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
