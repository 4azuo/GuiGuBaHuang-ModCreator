using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.Windows;
using ModCreator.WindowData;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for ModEventFile management operations in ProjectEditorWindow.Tab5
    /// Handles CRUD operations for ModEvent files and folders
    /// </summary>
    public class ModEventFileManagementBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private readonly Window _owner;

        public ModEventFileManagementBusiness(ProjectEditorWindowData windowData, Window owner)
        {
            _windowData = windowData ?? throw new ArgumentNullException(nameof(windowData));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        #region Folder Operations

        /// <summary>
        /// Creates a new folder for ModEvent files
        /// </summary>
        public void CreateModEventFolder()
        {
            var modPath = GetModPath();
            if (modPath == null) return;

            string parentPath = modPath;
            if (_windowData.SelectedEventItem != null)
            {
                parentPath = _windowData.SelectedEventItem.IsFolder
                    ? _windowData.SelectedEventItem.FullPath
                    : Path.GetDirectoryName(_windowData.SelectedEventItem.FullPath);
            }

            var inputWindow = new InputWindow
            {
                Owner = _owner
            };
            inputWindow.WindowData.WindowTitle = MessageHelper.Get("Messages.Dialogs.CreateFolder.Title") ?? "Create New Folder";
            inputWindow.WindowData.Label = MessageHelper.Get("Messages.Dialogs.CreateFolder.Label") ?? "Folder name:";
            inputWindow.WindowData.InputValue = MessageHelper.Get("Messages.Dialogs.CreateFolder.DefaultValue") ?? "NewFolder";

            if (inputWindow.ShowDialog() != true) return;

            var folderName = inputWindow.WindowData.InputValue;

            if (!ValidateFolderName(folderName, out string errorMessage))
            {
                ShowWarning(errorMessage);
                return;
            }

            var newFolderPath = Path.Combine(parentPath, folderName);

            if (Directory.Exists(newFolderPath))
            {
                ShowWarning(MessageHelper.GetFormat("Messages.Error.FolderAlreadyExists", folderName));
                return;
            }

            Directory.CreateDirectory(newFolderPath);
            _windowData.LoadModEventFiles();
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.CreatedModEventFolder", folderName);
            ShowSuccess(MessageHelper.GetFormat("Messages.Success.FolderCreated", folderName));
        }

        /// <summary>
        /// Deletes an existing ModEvent folder
        /// </summary>
        public void DeleteModEventFolder()
        {
            if (_windowData.SelectedEventItem == null || !_windowData.SelectedEventItem.IsFolder)
            {
                ShowWarning(MessageHelper.Get("Messages.Warning.NoFolderSelected") ?? "No folder selected");
                return;
            }

            var folderPath = _windowData.SelectedEventItem.FullPath;
            var folderName = _windowData.SelectedEventItem.Name;

            if (!Directory.Exists(folderPath))
            {
                ShowWarning(MessageHelper.GetFormat("Messages.Error.FolderDoesNotExist", folderName));
                _windowData.LoadModEventFiles();
                return;
            }

            var hasContents = Directory.GetFileSystemEntries(folderPath).Length > 0;
            var warningMessage = hasContents
                ? MessageHelper.GetFormat("Messages.Confirmation.DeleteFolder", folderName)
                : MessageHelper.GetFormat("Messages.Confirmation.DeleteFolderEmpty", folderName);

            if (!ShowConfirmation(warningMessage)) return;

            Directory.Delete(folderPath, true);
            _windowData.LoadModEventFiles();
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedModEventFolder", folderName);
            ShowSuccess(MessageHelper.GetFormat("Messages.Success.FolderDeleted", folderName));
        }

        /// <summary>
        /// Opens the ModEvent folder in Windows Explorer
        /// </summary>
        public void OpenModEventFolder()
        {
            var modPath = GetModPath();
            if (modPath == null) return;

            Directory.CreateDirectory(modPath);
            System.Diagnostics.Process.Start("explorer.exe", modPath);
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.OpenedModEventFolder", modPath);
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Creates a new ModEvent file
        /// </summary>
        public void CreateModEvent()
        {
            var inputWindow = new InputWindow
            {
                Owner = _owner
            };
            inputWindow.WindowData.WindowTitle = MessageHelper.Get("Messages.Dialogs.CreateModEvent.Title");
            inputWindow.WindowData.Label = MessageHelper.Get("Messages.Dialogs.CreateModEvent.Label");
            inputWindow.WindowData.InputValue = MessageHelper.Get("Messages.Dialogs.CreateModEvent.DefaultValue");

            if (inputWindow.ShowDialog() != true) return;

            var className = inputWindow.WindowData.InputValue;

            if (!ValidateClassName(className, out string errorMessage))
            {
                ShowWarning(errorMessage);
                return;
            }

            var modPath = GetModPath();
            if (modPath == null) return;

            string targetPath = modPath;
            if (_windowData.SelectedEventItem != null)
            {
                targetPath = _windowData.SelectedEventItem.IsFolder
                    ? _windowData.SelectedEventItem.FullPath
                    : Path.GetDirectoryName(_windowData.SelectedEventItem.FullPath);
            }

            Directory.CreateDirectory(targetPath);

            var filePath = Path.Combine(targetPath, $"{className}.cs");

            if (File.Exists(filePath))
            {
                ShowWarning(MessageHelper.GetFormat("Messages.Error.ClassNameExists", className));
                return;
            }

            var maxOrder = _windowData.EventItems
                .Where(x => !x.IsFolder && x.GetObjectContentAs<ModEventItem>() != null)
                .Select(x => x.GetObjectContentAs<ModEventItem>().OrderIndex)
                .DefaultIfEmpty(0)
                .Max();

            var newEvent = new ModEventItem
            {
                OrderIndex = maxOrder + 1,
                CacheType = "Local",
                WorkOn = "Local",
                SelectedEvent = "OnTimeUpdate1000ms",
                FilePath = filePath
            };

            // Note: Code generation moved to Tab5CodeGenerationBusiness
            File.WriteAllText(filePath, string.Empty);
            _windowData.Project.ModEvents.Add(newEvent);
            _windowData.LoadModEventFiles();
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.CreatedModEvent", className);
            ShowSuccess(MessageHelper.GetFormat("Messages.Success.ModEventCreated", className));
        }

        /// <summary>
        /// Clones an existing ModEvent file
        /// </summary>
        public void CloneModEvent()
        {
            if (_windowData.SelectedModEvent == null)
            {
                ShowWarning(MessageHelper.Get("Messages.Warning.NoModEventSelected") ?? "No ModEvent selected");
                return;
            }

            var newClassName = $"{_windowData.SelectedModEvent.FileName}_Copy";

            var inputWindow = new InputWindow
            {
                Owner = _owner
            };
            inputWindow.WindowData.WindowTitle = MessageHelper.Get("Messages.Dialogs.CloneModEvent.Title") ?? "Clone ModEvent";
            inputWindow.WindowData.Label = MessageHelper.Get("Messages.Dialogs.CloneModEvent.Label") ?? "New class name:";
            inputWindow.WindowData.InputValue = newClassName;

            if (inputWindow.ShowDialog() != true) return;

            newClassName = inputWindow.WindowData.InputValue;

            if (!ValidateClassName(newClassName, out string errorMessage))
            {
                ShowWarning(errorMessage);
                return;
            }

            var modPath = Path.GetDirectoryName(_windowData.SelectedModEvent.FilePath);
            var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

            if (File.Exists(newFilePath))
            {
                ShowWarning(MessageHelper.GetFormat("Messages.Error.ClassNameExists", newClassName));
                return;
            }

            var clonedEvent = CloneModEventItem(_windowData.SelectedModEvent, newFilePath);

            // Note: Code generation moved to Tab5CodeGenerationBusiness
            File.WriteAllText(newFilePath, string.Empty);
            _windowData.LoadModEventFiles();
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.ClonedModEvent", newClassName);
            ShowSuccess(MessageHelper.Get("Messages.Success.ModEventCloned"));
        }

        /// <summary>
        /// Renames an existing ModEvent file
        /// </summary>
        public void RenameModEvent()
        {
            if (_windowData.SelectedModEvent == null)
            {
                ShowWarning(MessageHelper.Get("Messages.Warning.NoModEventSelected") ?? "No ModEvent selected");
                return;
            }

            var oldClassName = _windowData.SelectedModEvent.FileName;

            var inputWindow = new InputWindow
            {
                Owner = _owner
            };
            inputWindow.WindowData.WindowTitle = MessageHelper.Get("Messages.Dialogs.RenameModEvent.Title") ?? "Rename ModEvent";
            inputWindow.WindowData.Label = MessageHelper.Get("Messages.Dialogs.RenameModEvent.Label") ?? "New class name:";
            inputWindow.WindowData.InputValue = oldClassName;

            if (inputWindow.ShowDialog() != true) return;

            var newClassName = inputWindow.WindowData.InputValue;

            if (newClassName == oldClassName) return;

            if (!ValidateClassName(newClassName, out string errorMessage))
            {
                ShowWarning(errorMessage);
                return;
            }

            var oldFilePath = _windowData.SelectedModEvent.FilePath;
            var modPath = Path.GetDirectoryName(oldFilePath);
            var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

            if (File.Exists(newFilePath))
            {
                ShowWarning(MessageHelper.GetFormat("Messages.Error.ClassNameExists", newClassName));
                return;
            }

            _windowData.SelectedModEvent.FilePath = newFilePath;
            _windowData.SaveModEvent(_windowData.SelectedEventItem);

            if (File.Exists(oldFilePath))
                File.Delete(oldFilePath);

            _windowData.LoadModEventFiles();
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.RenamedModEvent", oldClassName, newClassName);
            ShowSuccess(MessageHelper.Get("Messages.Success.ModEventRenamed"));
        }

        /// <summary>
        /// Deletes an existing ModEvent file
        /// </summary>
        public void DeleteModEvent()
        {
            if (_windowData.SelectedModEvent == null)
            {
                ShowWarning(MessageHelper.Get("Messages.Warning.NoModEventSelected") ?? "No ModEvent selected");
                return;
            }

            var confirmMessage = MessageHelper.GetFormat("Messages.Confirmation.DeleteModEvent", _windowData.SelectedModEvent.FileName);
            if (!ShowConfirmation(confirmMessage)) return;

            var filePath = _windowData.SelectedModEvent.FilePath;
            var fileName = _windowData.SelectedModEvent.FileName;

            if (File.Exists(filePath))
                File.Delete(filePath);

            _windowData.LoadModEventFiles();
            _windowData.SelectedModEvent = null;
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedModEvent", fileName);
            ShowSuccess(MessageHelper.Get("Messages.Success.ModEventDeleted"));
        }

        /// <summary>
        /// Saves the currently selected ModEvent file
        /// </summary>
        public void SaveModEvent()
        {
            if (_windowData.SelectedModEvent == null)
            {
                ShowWarning(MessageHelper.Get("Messages.Warning.NoModEventSelected") ?? "No ModEvent selected");
                return;
            }

            _windowData.SaveModEvent(_windowData.SelectedEventItem);
            _windowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEvent", _windowData.SelectedModEvent?.FileName);
            ShowSuccess(MessageHelper.Get("Messages.Success.ModEventSaved"));
        }

        /// <summary>
        /// Switches the selected ModEvent to code-only mode
        /// </summary>
        public void SwitchToCodeMode()
        {
            if (_windowData?.SelectedModEvent == null) return;

            if (!_windowData.SelectedModEvent.IsCodeModeOnly)
            {
                var warningMessage = MessageHelper.Get("Messages.Warning.SwitchToCodeModeWarning");
                if (!ShowConfirmation(warningMessage)) return;

                _windowData.SelectedModEvent.IsCodeModeOnly = true;
                _windowData.SelectedModEvent.SelectedEvent = string.Empty;
                _windowData.SelectedModEvent.Conditions.Clear();
                _windowData.SelectedModEvent.Actions.Clear();
            }

            _windowData.StatusMessage = MessageHelper.Get("Messages.Success.SwitchedToCodeMode");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the Mod path from the project
        /// </summary>
        private string GetModPath()
        {
            if (_windowData?.Project == null)
            {
                ShowWarning(MessageHelper.Get("Messages.Warning.NoProjectLoaded") ?? "No project loaded");
                return null;
            }

            return Path.Combine(_windowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");
        }

        /// <summary>
        /// Validates a folder name
        /// </summary>
        private bool ValidateFolderName(string folderName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                errorMessage = MessageHelper.Get("Messages.Error.FolderNameEmpty");
                return false;
            }

            if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errorMessage = MessageHelper.Get("Messages.Error.FolderNameInvalidChars");
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates a C# class name
        /// </summary>
        private bool ValidateClassName(string className, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(className) || 
                !System.Text.RegularExpressions.Regex.IsMatch(className, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                errorMessage = MessageHelper.Get("Messages.Error.InvalidClassName");
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Creates a deep clone of a ModEventItem
        /// </summary>
        private ModEventItem CloneModEventItem(ModEventItem source, string newFilePath)
        {
            var clonedEvent = new ModEventItem
            {
                OrderIndex = source.OrderIndex,
                CacheType = source.CacheType,
                WorkOn = source.WorkOn,
                SelectedEvent = source.SelectedEvent,
                FilePath = newFilePath
            };

            foreach (var condition in source.Conditions)
            {
                clonedEvent.Conditions.Add(CloneEventAction(condition));
            }

            foreach (var action in source.Actions)
            {
                clonedEvent.Actions.Add(CloneEventAction(action));
            }

            return clonedEvent;
        }

        /// <summary>
        /// Creates a deep clone of an EventActionBase
        /// </summary>
        private EventActionBase CloneEventAction(EventActionBase source)
        {
            var cloned = new EventActionBase
            {
                Name = source.Name,
                Category = source.Category,
                DisplayName = source.DisplayName,
                Description = source.Description,
                Code = source.Code
            };

            foreach (var child in source.Children)
            {
                cloned.Children.Add(CloneEventAction(child));
            }

            return cloned;
        }

        /// <summary>
        /// Shows a warning message box
        /// </summary>
        private void ShowWarning(string message)
        {
            MessageBox.Show(
                message, 
                MessageHelper.Get("Messages.Warning.Title"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }

        /// <summary>
        /// Shows a success message box
        /// </summary>
        private void ShowSuccess(string message)
        {
            MessageBox.Show(
                message, 
                MessageHelper.Get("Messages.Success.Title"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        private bool ShowConfirmation(string message)
        {
            var result = MessageBox.Show(
                message, 
                MessageHelper.Get("Messages.Confirmation.Title"), 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        #endregion
    }
}
