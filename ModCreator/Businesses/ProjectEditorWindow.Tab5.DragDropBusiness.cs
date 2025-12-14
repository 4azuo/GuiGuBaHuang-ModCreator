using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Common business logic for drag and drop operations in TreeView
    /// </summary>
    public abstract class TreeViewDragDropBusinessBase
    {
        protected readonly TreeView _treeView;
        protected readonly ProjectEditorWindowData _windowData;
        
        // Drag/Drop state
        protected EventActionBase _draggedItem;
        protected System.Windows.Controls.Primitives.Popup _dropIndicatorPopup;
        protected TreeViewItem _lastTargetItem;
        protected DropPosition _lastDropPosition;

        protected TreeViewDragDropBusinessBase(TreeView treeView, ProjectEditorWindowData windowData)
        {
            _treeView = treeView;
            _windowData = windowData;
        }

        /// <summary>
        /// Get the root collection for the TreeView
        /// </summary>
        protected abstract ObservableCollection<EventActionBase> GetRootCollection();

        /// <summary>
        /// Get the success message key for reordering
        /// </summary>
        protected abstract string GetSuccessMessageKey();

        public void OnPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeViewItem = FindVisualParent<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                _draggedItem = treeViewItem.DataContext as EventActionBase;
            }
        }

        public void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && _draggedItem != null)
            {
                if (_draggedItem.Name == Constants.EventActionRootElement.Name || _draggedItem.IsHidden)
                {
                    _draggedItem = null;
                    return;
                }

                DragDrop.DoDragDrop(_treeView, _draggedItem, DragDropEffects.Move);
                RemoveDropIndicator();
                _draggedItem = null;
            }
            else
            {
                RemoveDropIndicator();
            }
        }

        public void OnDragOver(DragEventArgs e)
        {
            if (_draggedItem == null) return;

            var treeViewItem = FindVisualParent<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (treeViewItem == null)
            {
                RemoveDropIndicator();
                return;
            }

            var targetItem = treeViewItem.DataContext as EventActionBase;
            if (targetItem == null || targetItem == _draggedItem || targetItem.Name == Constants.EventActionRootElement.Name)
            {
                RemoveDropIndicator();
                e.Effects = DragDropEffects.None;
                return;
            }

            // Allow dropping on hidden items only if they can accept children (like Then/Else blocks)
            if (targetItem.IsHidden && !targetItem.IsCanAddChild)
            {
                RemoveDropIndicator();
                e.Effects = DragDropEffects.None;
                return;
            }

            // Check if dragged item is ancestor of target (prevent circular reference)
            if (IsAncestor(_draggedItem, targetItem))
            {
                RemoveDropIndicator();
                e.Effects = DragDropEffects.None;
                return;
            }

            var dropPosition = GetDropPosition(treeViewItem, e.GetPosition(treeViewItem), targetItem.IsCanAddChild);
            ShowDropIndicator(treeViewItem, dropPosition);

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        public void OnDrop(DragEventArgs e)
        {
            RemoveDropIndicator();

            if (_draggedItem == null || _windowData?.SelectedModEvent == null) return;

            var treeViewItem = FindVisualParent<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (treeViewItem == null) return;

            var targetItem = treeViewItem.DataContext as EventActionBase;
            if (targetItem == null || targetItem == _draggedItem) return;

            // Cannot drop on root element
            if (targetItem.Name == Constants.EventActionRootElement.Name)
            {
                _draggedItem = null;
                return;
            }

            // Cannot drop on hidden items unless they can accept children (like Then/Else blocks)
            if (targetItem.IsHidden && !targetItem.IsCanAddChild)
            {
                _draggedItem = null;
                return;
            }

            // Check if dragged item is ancestor of target
            if (IsAncestor(_draggedItem, targetItem))
            {
                _draggedItem = null;
                return;
            }

            var dropPosition = GetDropPosition(treeViewItem, e.GetPosition(treeViewItem), targetItem.IsCanAddChild);

            // Remove from current location
            if (_draggedItem.Parent != null)
            {
                _draggedItem.Parent.Children.Remove(_draggedItem);
            }
            else
            {
                GetRootCollection().Remove(_draggedItem);
            }

            // Add to new location based on drop position
            if (dropPosition == DropPosition.Inside && targetItem.IsCanAddChild)
            {
                // Add as child - position depends on where mouse was
                var mousePos = e.GetPosition(treeViewItem);
                var relativeY = mousePos.Y;
                if (relativeY < treeViewItem.ActualHeight * 0.5)
                {
                    // Upper half - insert at beginning
                    targetItem.Children.Insert(0, _draggedItem);
                }
                else
                {
                    // Lower half - add at end
                    targetItem.Children.Add(_draggedItem);
                }
            }
            else
            {
                // Add as sibling (above or below)
                List<EventActionBase> parentCollection;
                if (targetItem.Parent != null)
                {
                    parentCollection = targetItem.Parent.Children;
                }
                else
                {
                    var root = GetRootCollection().FirstOrDefault(c => c.Name == Constants.EventActionRootElement.Name);
                    parentCollection = root?.Children;
                }

                if (parentCollection != null)
                {
                    int targetIndex = parentCollection.IndexOf(targetItem);
                    if (targetIndex >= 0)
                    {
                        if (dropPosition == DropPosition.Below)
                        {
                            parentCollection.Insert(targetIndex + 1, _draggedItem);
                        }
                        else // Above
                        {
                            parentCollection.Insert(targetIndex, _draggedItem);
                        }
                    }
                }
            }

            _windowData.StatusMessage = MessageHelper.Get(GetSuccessMessageKey());
            _draggedItem = null;
        }

        protected void RemoveDropIndicator()
        {
            if (_dropIndicatorPopup != null)
            {
                _dropIndicatorPopup.IsOpen = false;
                _dropIndicatorPopup = null;
            }
            _lastTargetItem = null;
        }

        protected void ShowDropIndicator(TreeViewItem targetItem, DropPosition position)
        {
            if (targetItem == null) return;

            // Remove previous indicator
            RemoveDropIndicator();

            // Calculate position
            var itemPosition = targetItem.PointToScreen(new Point(0, 0));
            var treeViewPosition = _treeView.PointToScreen(new Point(0, 0));

            double y;
            double indent = 0;

            if (position == DropPosition.Above)
            {
                y = itemPosition.Y - 1;
            }
            else if (position == DropPosition.Below)
            {
                y = itemPosition.Y + targetItem.ActualHeight - 1;
            }
            else // Inside
            {
                y = itemPosition.Y + 2;
                indent = 20; // Indent to show it's going inside
            }

            // Create indicator line
            var line = new System.Windows.Shapes.Rectangle
            {
                Fill = Brushes.Black,
                Height = 2,
                Width = targetItem.ActualWidth - indent,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Create popup
            _dropIndicatorPopup = new System.Windows.Controls.Primitives.Popup
            {
                Child = line,
                PlacementTarget = _treeView,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute,
                HorizontalOffset = itemPosition.X - treeViewPosition.X + indent,
                VerticalOffset = y - treeViewPosition.Y,
                IsOpen = true,
                AllowsTransparency = true,
                IsHitTestVisible = false
            };

            _lastTargetItem = targetItem;
            _lastDropPosition = position;
        }

        #region Helper Methods

        /// <summary>
        /// Determines the drop position based on mouse position within a TreeViewItem
        /// </summary>
        protected DropPosition GetDropPosition(TreeViewItem item, Point mousePosition, bool canAddChild)
        {
            var height = item.ActualHeight;
            var relativeY = mousePosition.Y;

            // Narrow zone for Inside (only very center): top 40%, middle 20%, bottom 40%
            // This allows dropping as sibling more easily while still supporting drop inside
            if (canAddChild && relativeY > height * 0.4 && relativeY < height * 0.6)
            {
                return DropPosition.Inside;
            }
            else if (relativeY < height * 0.5)
            {
                return DropPosition.Above;
            }
            else
            {
                return DropPosition.Below;
            }
        }

        /// <summary>
        /// Checks if potentialAncestor is an ancestor of item (prevents circular references)
        /// </summary>
        protected bool IsAncestor(EventActionBase potentialAncestor, EventActionBase item)
        {
            if (potentialAncestor == null || item == null) return false;

            var current = item.Parent;
            while (current != null)
            {
                if (current == potentialAncestor)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        /// <summary>
        /// Finds a visual parent of the specified type in the visual tree
        /// </summary>
        protected T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Drag/Drop business logic for Conditions TreeView
    /// </summary>
    public class ConditionsDragDropBusiness : TreeViewDragDropBusinessBase
    {
        public ConditionsDragDropBusiness(TreeView treeView, ProjectEditorWindowData windowData)
            : base(treeView, windowData)
        {
        }

        protected override ObservableCollection<EventActionBase> GetRootCollection()
        {
            return _windowData?.SelectedModEvent?.Conditions;
        }

        protected override string GetSuccessMessageKey()
        {
            return "Messages.Success.ReorderedConditions";
        }
    }

    /// <summary>
    /// Drag/Drop business logic for Actions TreeView
    /// </summary>
    public class ActionsDragDropBusiness : TreeViewDragDropBusinessBase
    {
        public ActionsDragDropBusiness(TreeView treeView, ProjectEditorWindowData windowData)
            : base(treeView, windowData)
        {
        }

        protected override ObservableCollection<EventActionBase> GetRootCollection()
        {
            return _windowData?.SelectedModEvent?.Actions;
        }

        protected override string GetSuccessMessageKey()
        {
            return "Messages.Success.ReorderedActions";
        }
    }
}
