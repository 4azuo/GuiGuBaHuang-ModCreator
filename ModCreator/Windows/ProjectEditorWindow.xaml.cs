using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Threading;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        // Auto-save timer
        private DispatcherTimer _autoSaveTimer;

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

                // Setup AvalonEdit binding
                SetupAvalonEditBinding();
                
                // Setup Variables Source Editor binding
                SetupVariablesSourceBinding();

                // Initialize auto-save timer
                InitializeAutoSaveTimer();
            }

            // Subscribe to Closed event to notify parent window
            Closed += ProjectEditorWindow_Closed;
        }

        private void InitializeAutoSaveTimer()
        {
            _autoSaveTimer = new DispatcherTimer();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(30);
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;

            if (WindowData?.Project?.AutoSaveEnabled == true)
            {
                _autoSaveTimer.Start();
            }
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (WindowData?.Project?.AutoSaveEnabled == true)
            {
                WindowData.SaveProject();
                WindowData.StatusMessage = MessageHelper.Get("Messages.Success.AutoSavedProject");
            }
        }

        private void AutoSave_Changed(object sender, RoutedEventArgs e)
        {
            if (_autoSaveTimer == null) return;

            if (WindowData?.Project?.AutoSaveEnabled == true)
            {
                _autoSaveTimer.Start();
                WindowData.StatusMessage = MessageHelper.Get("Messages.Success.AutoSaveEnabled");
            }
            else
            {
                _autoSaveTimer.Stop();
                WindowData.StatusMessage = MessageHelper.Get("Messages.Success.AutoSaveDisabled");
            }
        }

        private void ProjectEditorWindow_Closed(object sender, EventArgs e)
        {
            // Dispose auto-save timer
            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Stop();
                _autoSaveTimer.Tick -= AutoSaveTimer_Tick;
                _autoSaveTimer = null;
            }

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
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.UpdatedProject", WindowData.Project.ProjectName);
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
                WindowData.StatusMessage = MessageHelper.Get("Messages.Info.Ready");
                return;
            }

            switch (tabControl.SelectedIndex)
            {
                case 1: WindowData.LoadConfFiles(); WindowData.StatusMessage = MessageHelper.Get("Messages.Success.RefreshedConfFiles"); break;
                case 2: WindowData.LoadImageFiles(); WindowData.StatusMessage = MessageHelper.Get("Messages.Success.RefreshedImageFiles"); break;
                case 3: WindowData.LoadGlobalVariables(); WindowData.StatusMessage = MessageHelper.Get("Messages.Success.RefreshedGlobalVariables"); break;
                case 4: WindowData.LoadModEventFiles(); WindowData.StatusMessage = MessageHelper.Get("Messages.Success.RefreshedModEventFiles"); break;
                default: WindowData.ReloadProjectData(); WindowData.StatusMessage = MessageHelper.Get("Messages.Info.Ready"); break;
            }
        }
    }
}