using ModCreator.Businesses;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        private ImageFilesDragDropBusiness _imageFilesDragDropBusiness;

        private void CreateImageFolder_Click(object sender, RoutedEventArgs e)
        {
            var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
            
            string parentPath = imgPath;
            if (WindowData.SelectedCustomResourceItem != null)
            {
                parentPath = WindowData.SelectedCustomResourceItem.IsFolder
                    ? WindowData.SelectedCustomResourceItem.FullPath
                    : Path.GetDirectoryName(WindowData.SelectedCustomResourceItem.FullPath);
            }

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { 
                    WindowTitle = MessageHelper.Get("Messages.Dialogs.CreateFolder.Title"),
                    Label = MessageHelper.Get("Messages.Dialogs.CreateFolder.Label"),
                    InputValue = MessageHelper.Get("Messages.Dialogs.CreateFolder.DefaultValue")
                }
            };

            if (inputWindow.ShowDialog() != true) return;

            var folderName = inputWindow.WindowData.InputValue;

            if (string.IsNullOrWhiteSpace(folderName))
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.FolderNameEmpty"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.FolderNameInvalidChars"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newFolderPath = Path.Combine(parentPath, folderName);

            if (Directory.Exists(newFolderPath))
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.FolderAlreadyExists", folderName), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Directory.CreateDirectory(newFolderPath);
            WindowData.LoadCustomResourceFiles();
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.CreatedImageFolder", folderName);
            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.FolderCreated", folderName), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteImageFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedCustomResourceItem == null || !WindowData.SelectedCustomResourceItem.IsFolder) return;

            var folderPath = WindowData.SelectedCustomResourceItem.FullPath;
            var folderName = WindowData.SelectedCustomResourceItem.Name;

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.FolderDoesNotExist", folderName), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                WindowData.LoadCustomResourceFiles();
                return;
            }

            var hasContents = Directory.GetFileSystemEntries(folderPath).Length > 0;
            var warningMessage = hasContents
                ? $"Are you sure you want to delete folder '{folderName}' and all its contents?"
                : $"Are you sure you want to delete folder '{folderName}'?";

            var result = MessageBox.Show(warningMessage, "Delete Folder", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (!FileHelper.DeleteFolderSafe(folderPath))
                {
                    WindowData.LoadCustomResourceFiles();
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedImageFolder", folderName);
                    MessageBox.Show(MessageHelper.GetFormat("Messages.Success.FolderDeleted", folderName), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Error.FolderDeletionFailed", folderName);
                    MessageBox.Show(MessageHelper.GetFormat("Messages.Error.FolderDeletionFailed", folderName), MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            var allExtensions = WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}")
                .Concat(WindowData.AudioExtensions.Select(ext => $"*{ext.Extension}"));
            
            using (var dialog = new OpenFileDialog
            {
                Filter = $"Resource Files|{string.Join(";", allExtensions)}",
                Title = "Select Resource to Import",
                Multiselect = true
            })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
                string targetPath = imgPath;
                
                if (WindowData.SelectedCustomResourceItem != null)
                {
                    targetPath = WindowData.SelectedCustomResourceItem.IsFolder
                        ? WindowData.SelectedCustomResourceItem.FullPath
                        : Path.GetDirectoryName(WindowData.SelectedCustomResourceItem.FullPath);
                }
                
                Directory.CreateDirectory(targetPath);

                foreach (var file in dialog.FileNames)
                {
                    var destPath = Path.Combine(targetPath, Path.GetFileName(file));
                    File.Copy(file, destPath, true);
                }

                WindowData.LoadCustomResourceFiles();
                WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.ImportedImages", dialog.FileNames.Length);
            }
        }

        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedCustomResourceItem.RelativePath)) return;

            var allExtensions = WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}")
                .Concat(WindowData.AudioExtensions.Select(ext => $"*{ext.Extension}"));

            using (var dialog = new SaveFileDialog
            {
                FileName = WindowData.SelectedCustomResourceItem.RelativePath,
                Filter = $"Resource Files|{string.Join(";", allExtensions)}",
                Title = "Export Resource"
            })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                var sourcePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedCustomResourceItem.RelativePath);
                File.Copy(sourcePath, dialog.FileName, true);
                WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.ExportedImage", WindowData.SelectedCustomResourceItem.RelativePath);
                MessageBox.Show(MessageHelper.Get("Messages.Success.ImageExported"), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowData.HasSelectedCustomResourceItem) return;

            var result = MessageBox.Show(MessageHelper.GetFormat("Messages.Confirmation.DeleteImage", WindowData.SelectedCustomResourceItem.RelativePath), MessageHelper.Get("Messages.Confirmation.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedCustomResourceItem.RelativePath);
                var fileName = WindowData.SelectedCustomResourceItem.RelativePath;
                WindowData.SelectedCustomResourceItem.RelativePath = null;
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    WindowData.LoadCustomResourceFiles();
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedImage", fileName);
                    MessageBox.Show(MessageHelper.GetFormat("Messages.Success.DeletedImage", fileName), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Error.FileNotFound", fileName);
                    MessageBox.Show(MessageHelper.GetFormat("Messages.Error.FileNotFound", fileName), MessageHelper.Get("Messages.Error.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenModImgFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.Project == null) return;
            
            var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
            Directory.CreateDirectory(imgPath);
            System.Diagnostics.Process.Start("explorer.exe", imgPath);
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.OpenedModImgFolder", imgPath);
        }

        private void TreeView_ImageSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Stop any playing audio when selection changes
            audioPlayerControl?.Cleanup();

            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedCustomResourceItem = fileItem;
            }
        }

        private void TreeView_GameResourceSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Stop any playing audio when selection changes
            audioPlayerControl?.Cleanup();
            
            if (e.NewValue is GameResourceItem resourceItem)
            {
                WindowData.SelectedGameResourceItem = resourceItem;
            }
        }

        #region Image Files Drag/Drop Event Handlers

        private void ImageFiles_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _imageFilesDragDropBusiness?.OnPreviewMouseLeftButtonDown(sender, e);
        }

        private void ImageFiles_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _imageFilesDragDropBusiness?.OnMouseMove(sender, e);
        }

        private void ImageFiles_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            _imageFilesDragDropBusiness?.OnDragOver(sender, e);
        }

        private void ImageFiles_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _imageFilesDragDropBusiness?.OnDrop(sender, e);
        }

        #endregion
    }
}