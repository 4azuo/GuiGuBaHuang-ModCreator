using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for drag and drop operations in ImageFiles TreeView (Tab3)
    /// </summary>
    public class ImageFilesDragDropBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private Point _dragStartPoint;
        private FileItem _draggedItem;

        public ImageFilesDragDropBusiness(ProjectEditorWindowData windowData)
        {
            _windowData = windowData ?? throw new ArgumentNullException(nameof(windowData));
        }

        /// <summary>
        /// Handles PreviewMouseLeftButtonDown event to prepare for drag
        /// </summary>
        public void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        /// <summary>
        /// Handles MouseMove event to start drag operation
        /// </summary>
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem == null)
            {
                Point currentPosition = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPosition;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = sender as TreeView;
                    if (treeView == null) return;

                    var treeViewItem = FindVisualParent<TreeViewItem>((DependencyObject)e.OriginalSource);
                    if (treeViewItem != null)
                    {
                        _draggedItem = treeView.SelectedItem as FileItem;
                        if (_draggedItem != null && !_draggedItem.IsFolder)
                        {
                            DragDrop.DoDragDrop(treeViewItem, _draggedItem, DragDropEffects.Move);
                            _draggedItem = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles DragOver event to provide visual feedback
        /// </summary>
        public void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            var data = e.Data.GetData(typeof(FileItem)) as FileItem;
            if (data == null) return;

            var treeViewItem = FindVisualParent<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem != null)
            {
                var targetItem = treeViewItem.DataContext as FileItem;
                if (targetItem != null && targetItem.IsFolder && targetItem != data)
                {
                    e.Effects = DragDropEffects.Move;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles Drop event to move file into folder
        /// </summary>
        public void OnDrop(object sender, DragEventArgs e)
        {
            var draggedFile = e.Data.GetData(typeof(FileItem)) as FileItem;
            if (draggedFile == null || draggedFile.IsFolder) return;

            var treeViewItem = FindVisualParent<TreeViewItem>((DependencyObject)e.OriginalSource);
            if (treeViewItem == null) return;

            var targetFolder = treeViewItem.DataContext as FileItem;
            if (targetFolder == null || !targetFolder.IsFolder || targetFolder == draggedFile) return;

            // Perform file move operation
            MoveFileToFolder(draggedFile, targetFolder);

            e.Handled = true;
        }

        /// <summary>
        /// Moves a file to a target folder
        /// </summary>
        private void MoveFileToFolder(FileItem file, FileItem targetFolder)
        {
            try
            {
                if (_windowData?.Project == null)
                {
                    MessageBox.Show("Project not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var sourceFullPath = file.FullPath;
                var targetFullPath = Path.Combine(targetFolder.FullPath, file.Name);

                if (!File.Exists(sourceFullPath))
                {
                    MessageBox.Show($"Source file not found:\n{sourceFullPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (File.Exists(targetFullPath))
                {
                    var result = MessageBox.Show(
                        $"A file with the same name already exists in the target folder:\n{file.Name}\n\nDo you want to overwrite it?",
                        "File Exists",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes) return;
                }

                // Move the file
                File.Move(sourceFullPath, targetFullPath, true);

                // Reload image files
                _windowData.LoadCustomResourceFiles();

                _windowData.StatusMessage = $"Moved file: {file.Name} → {targetFolder.RelativePath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to move file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DebugHelper.Error($"Failed to move image file: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds a visual parent of the specified type in the visual tree
        /// </summary>
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = System.Windows.Media.VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
