using ModCreator.Businesses;
using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        // Auto-save timer
        private DispatcherTimer _autoSaveTimer;

        // Business logic handlers - declared in partial classes
        private Tab5CodeGenerationBusiness _tab5CodeGenerationBusiness;

        /// <summary>
        /// Project to edit - set before showing dialog
        /// </summary>
        public ModProject ProjectToEdit { get; set; }

        /// <summary>
        /// Event raised when the window is closed to notify parent window to refresh
        /// </summary>
        public event EventHandler ProjectUpdated;

        public override void OnLoad()
        {
            base.OnLoad();
            ProjectEditorWindow_Loaded();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ProjectEditorWindow_Closed();
        }

        private async void ProjectEditorWindow_Loaded()
        {
            // Now ProjectToEdit has been set by the caller
            if (ProjectToEdit != null)
            {
                WindowData.Project = ProjectToEdit;

                // Setup AvalonEdit binding
                SetupAvalonEditBinding();
                
                // Setup Variables Source Editor binding
                SetupVariablesSourceBinding();

                // Initialize auto-save timer
                InitializeAutoSaveTimer();

                // Setup Event Source Editor binding
                SetupEventSourceEditorBinding();

                // Initialize Business classes
                InitializeBusinesses();
                
                // Load game resources asynchronously
                await LoadGameResourcesAsync();
            }
        }

        private async System.Threading.Tasks.Task LoadGameResourcesAsync()
        {
            try
            {
                WindowData.IsLoadingGameResources = true;
                WindowData.StatusMessage = "Loading game resources...";
                
                var business = ProjectEditorWindowTab3GameResourceBusiness.Instance;
                var gameFolderPath = Constants.BaseGameResourcesDir;
                
                var (success, errors) = business.InitializeResourcesAsync();
                if (!success)
                {
                    WindowData.StatusMessage = "Failed to initialize game resources.";
                    WindowData.IsLoadingGameResources = false;
                    return;
                }
                
                // Load resources asynchronously
                await WindowData.LoadGameResourcesAsync();
                
                // Calculate totals
                int totalTextures = WindowData.Texture2DItems.Sum(i => CountItems(i));
                int totalSprites = WindowData.SpriteItems.Sum(i => CountItems(i));
                // int totalTextAssets = WindowData.TextAssetItems.Sum(i => CountItems(i));
                int totalAudioClips = WindowData.AudioClipItems.Sum(i => CountItems(i));
                int totalOther = WindowData.OtherItems.Sum(i => CountItems(i));
                int total = totalTextures + totalSprites + totalAudioClips + totalOther;
                
                WindowData.StatusMessage = $"Loaded {total} game resources (T2D:{totalTextures} Sprite:{totalSprites} Audio:{totalAudioClips} Other:{totalOther})";
            }
            catch (Exception ex)
            {
                WindowData.StatusMessage = $"Error loading game resources: {ex.Message}";
                DebugHelper.Log($"Failed to load game resources: {ex}");
            }
            finally
            {
                WindowData.IsLoadingGameResources = false;
            }
        }
        
        private int CountItems(GameResourceItem item)
        {
            if (item.IsFolder)
            {
                return item.Children.Sum(child => CountItems(child));
            }
            return 1;
        }

        private void InitializeBusinesses()
        {
            // Initialize Drag/Drop Business for Conditions and Actions TreeViews
            var tvConditions = this.FindName("tvConditions") as TreeView;
            var tvActions = this.FindName("tvActions") as TreeView;

            if (tvConditions != null && WindowData != null)
            {
                _conditionsDragDropBusiness = new ConditionsDragDropBusiness(tvConditions, WindowData);
            }
            
            if (tvActions != null && WindowData != null)
            {
                _actionsDragDropBusiness = new ActionsDragDropBusiness(tvActions, WindowData);
            }

            // Initialize ImageFiles Drag/Drop Business
            if (WindowData != null)
            {
                _imageFilesDragDropBusiness = new ImageFilesDragDropBusiness(WindowData);
            }

            // Initialize ModEventFile Management Business
            if (WindowData != null)
            {
                _modEventFileManagementBusiness = new ModEventFileManagementBusiness(WindowData, this);
            }

            // Initialize Condition and Action Management Businesses
            if (WindowData != null && tvConditions != null)
            {
                _conditionManagementBusiness = new ConditionManagementBusiness(WindowData, this, tvConditions);
            }
            
            if (WindowData != null && tvActions != null)
            {
                _actionManagementBusiness = new ActionManagementBusiness(WindowData, this, tvActions);
            }

            // Initialize Global Variables Drag/Drop Business
            if (WindowData != null)
            {
                _globalVariablesDragDropBusiness = new GlobalVariablesDragDropBusiness(WindowData);
            }

            // Initialize Tab4 Code Generation Business
            if (WindowData != null)
            {
                _tab4CodeGenerationBusiness = new Tab4CodeGenerationBusiness(WindowData);
            }

            // Initialize Tab5 Code Generation Business
            if (WindowData != null)
            {
                _tab5CodeGenerationBusiness = new Tab5CodeGenerationBusiness(WindowData, this);
            }
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

        private void ProjectEditorWindow_Closed()
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

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Get current tab
            var tabControl = this.FindName("tabControl") as System.Windows.Controls.TabControl;
            int currentTab = tabControl?.SelectedIndex ?? -1;

            // Handle Ctrl+Z for Undo
            if (e.Key == System.Windows.Input.Key.Z &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                if (currentTab == 3) // Tab4 - Global Variables
                {
                    if (WindowData?.CanUndoVariables == true)
                    {
                        e.Handled = true;
                        UndoVariables_Click(sender, e);
                    }
                }
                else if (currentTab == 4 && !WindowData.IsCodeModeOnly) // Tab5 - ModEvent
                {
                    if (WindowData?.CanUndo == true)
                    {
                        e.Handled = true;
                        Undo_Click(sender, e);
                    }
                }
            }
            // Handle Ctrl+Y for Redo
            else if (e.Key == System.Windows.Input.Key.Y &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                if (currentTab == 3) // Tab4 - Global Variables
                {
                    if (WindowData?.CanRedoVariables == true)
                    {
                        e.Handled = true;
                        RedoVariables_Click(sender, e);
                    }
                }
                else if (currentTab == 4 && !WindowData.IsCodeModeOnly) // Tab5 - ModEvent
                {
                    if (WindowData?.CanRedo == true)
                    {
                        e.Handled = true;
                        Redo_Click(sender, e);
                    }
                }
            }
            // Handle Ctrl+F to open replace panel
            else if (e.Key == System.Windows.Input.Key.F && 
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
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new Windows.HelperWindow { Owner = this };
            helpWindow.ShowDialog();
        }

        private void Donate_Click(object sender, RoutedEventArgs e)
        {
            var donateWindow = new Windows.DonateWindow { Owner = this };
            donateWindow.ShowDialog();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            WindowData?.UndoModEvent();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            WindowData?.RedoModEvent();
        }
        
        private void UndoVariables_Click(object sender, RoutedEventArgs e)
        {
            WindowData?.UndoVariables();
        }

        private void RedoVariables_Click(object sender, RoutedEventArgs e)
        {
            WindowData?.RedoVariables();
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
                case 2: WindowData.LoadCustomResourceFiles(); WindowData.StatusMessage = MessageHelper.Get("Messages.Success.RefreshedImageFiles"); break;
                case 3: WindowData.LoadGlobalVariables(); WindowData.StatusMessage = MessageHelper.Get("Messages.Success.RefreshedGlobalVariables"); break;
                case 4: WindowData.LoadModEventFiles(); WindowData.StatusMessage = MessageHelper.Get("Messages.Success.RefreshedModEventFiles"); break;
                default: WindowData.ReloadProjectData(); WindowData.StatusMessage = MessageHelper.Get("Messages.Info.Ready"); break;
            }
        }
    }
}