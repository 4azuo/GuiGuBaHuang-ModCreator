using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for drag and drop operations in Global Variables DataGrid (Tab4)
    /// </summary>
    public class GlobalVariablesDragDropBusiness
    {
        private readonly ProjectEditorWindowData _windowData;
        private GlobalVariable _draggedItem;

        public GlobalVariablesDragDropBusiness(ProjectEditorWindowData windowData)
        {
            _windowData = windowData ?? throw new System.ArgumentNullException(nameof(windowData));
        }

        /// <summary>
        /// Handles PreviewMouseLeftButtonDown on drag handle
        /// </summary>
        public void OnDragHandlePreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock == null) return;

            var row = FindVisualParent<DataGridRow>(textBlock);
            if (row != null)
            {
                _draggedItem = row.Item as GlobalVariable;
                if (_draggedItem != null)
                {
                    DragDrop.DoDragDrop(row, _draggedItem, DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        /// Handles Drop event on DataGrid
        /// </summary>
        public void OnDataGridDrop(object sender, DragEventArgs e)
        {
            if (_draggedItem == null) return;

            var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row == null) return;

            var targetItem = row.Item as GlobalVariable;
            if (targetItem == null || targetItem == _draggedItem) return;

            var container = _windowData?.GlobalVariablesContainer?.Variables;
            if (container == null) return;

            int oldIndex = container.IndexOf(_draggedItem);
            int newIndex = container.IndexOf(targetItem);

            if (oldIndex != -1 && newIndex != -1)
            {
                container.Move(oldIndex, newIndex);
                _windowData.StatusMessage = MessageHelper.Get("Messages.Success.ReorderedVariables");
            }

            _draggedItem = null;
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
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
