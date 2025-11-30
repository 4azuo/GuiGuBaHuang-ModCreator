using ModCreator.Businesses;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        // Business layer instances
        private ModConfBusiness _modConfBusiness;
        private ModImgBusiness _modImgBusiness;
        private GlobalVariablesBusiness _globalVariablesBusiness;
        private ModEventBusiness _modEventBusiness;

        /// <summary>
        /// Project to edit - set before showing dialog
        /// </summary>
        public ModProject ProjectToEdit { get; set; }

        /// <summary>
        /// Event raised when the window is closed to notify parent window to refresh
        /// </summary>
        public event EventHandler ProjectUpdated;

        [SupportedOSPlatform("windows6.1")]
        public override ProjectEditorWindowData InitData(CancelEventArgs e)
        {
            var data = new ProjectEditorWindowData();
            data.New();
            
            Loaded += ProjectEditorWindow_Loaded;
            
            return data;
        }

        [SupportedOSPlatform("windows6.1")]
        private void ProjectEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Now ProjectToEdit has been set by the caller
            if (ProjectToEdit != null && WindowData != null)
            {
                WindowData.Project = ProjectToEdit;

                // Initialize business layer
                _modConfBusiness = new ModConfBusiness(WindowData, this);
                _modImgBusiness = new ModImgBusiness(WindowData, this);
                _globalVariablesBusiness = new GlobalVariablesBusiness(WindowData, this);
                _modEventBusiness = new ModEventBusiness(WindowData, this);

                // Setup AvalonEdit binding
                SetupAvalonEditBinding();
                
                // Setup Variables Source Editor binding
                SetupVariablesSourceBinding();
                
                // Setup Title Image
                SetupTitleImage();
                
                // Populate Events ComboBox
                PopulateEventsComboBox();
            }

            // Subscribe to Closed event to notify parent window
            Closed += ProjectEditorWindow_Closed;
        }

        private void ProjectEditorWindow_Closed(object sender, EventArgs e)
        {
            // Notify parent window to refresh project list
            ProjectUpdated?.Invoke(this, EventArgs.Empty);
        }

        [SupportedOSPlatform("windows6.1")]
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle Ctrl+F to open replace panel
            if (e.Key == System.Windows.Input.Key.F && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                if (WindowData?.HasSelectedConfFile == true)
                {
                    e.Handled = true;
                    ReplaceInEditor_Click(sender, e);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to save all changes?",
                "Confirm Save",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            WindowData.SaveProject();
            MessageBox.Show(
                MessageHelper.GetFormat("Messages.Success.UpdatedProject", WindowData.Project.ProjectName), 
                MessageHelper.Get("Messages.Success.Title"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
            
            WindowData.BackupProject();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new Windows.HelperWindow { Owner = this };
            helpWindow.ShowDialog();
        }

        private void Translate_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedSourceLanguage == null)
            {
                MessageBox.Show("Please select a source language!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get translate script path from settings
            var settingsJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "settings.json"));
            var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsJson);
            
            if (!settings.TryGetValue("translateScriptPath", out var translateScriptPath))
            {
                MessageBox.Show("Translate script path not found in settings!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(translateScriptPath))
            {
                MessageBox.Show($"Translate script not found at: {translateScriptPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get python executable
            var pythonPath = PythonHelper.FindPythonExecutable();
            if (string.IsNullOrEmpty(pythonPath))
            {
                MessageBox.Show("Python executable not found! Please install Python 3 and add it to PATH.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Build command arguments
            var projectPath = WindowData.Project.ProjectPath;
            var sourceLanguage = WindowData.SelectedSourceLanguage.Code;
            var arguments = $"/K {pythonPath} \"{translateScriptPath}\" --project \"{projectPath}\" --path . --source_lan {sourceLanguage}";

            // Start translation process
            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = arguments,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(processInfo);
            
            MessageBox.Show($"Translation process started!\n\nProject: {WindowData.Project.ProjectName}\nSource Language: {WindowData.SelectedSourceLanguage.DisplayName}", 
                "Translation Started", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.HasUnsavedChanges())
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to discard them?",
                    "Confirm Cancel",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
                
                WindowData.RestoreProject();
            }

            Close();
        }

        [SupportedOSPlatform("windows6.1")]
        private void RefreshTab_Click(object sender, RoutedEventArgs e)
        {
            var tabControl = this.FindName("tabControl") as System.Windows.Controls.TabControl;
            if (tabControl == null || WindowData == null)
            {
                WindowData?.ReloadProjectData();
                return;
            }

            switch (tabControl.SelectedIndex)
            {
                case 1: WindowData.LoadConfFiles(); break;
                case 2: WindowData.LoadImageFiles(); break;
                case 3: WindowData.LoadGlobalVariables(); break;
                case 4: WindowData.LoadModEventFiles(); break;
                default: WindowData.ReloadProjectData(); break;
            }
        }
    }
}