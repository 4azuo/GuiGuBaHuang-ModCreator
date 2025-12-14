using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.Windows;
using ModCreator.WindowData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Base business logic for managing EventAction items in a TreeView
    /// </summary>
    public abstract class EventActionManagementBusinessBase
    {
        protected readonly ProjectEditorWindowData _windowData;
        protected readonly Window _owner;
        protected readonly TreeView _treeView;

        protected EventActionManagementBusinessBase(
            ProjectEditorWindowData windowData,
            Window owner,
            TreeView treeView)
        {
            _windowData = windowData ?? throw new ArgumentNullException(nameof(windowData));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _treeView = treeView;
        }

        /// <summary>
        /// Gets the collection to work with (Conditions or Actions)
        /// </summary>
        protected abstract ObservableCollection<EventActionBase> GetCollection();

        /// <summary>
        /// Gets the return type filter for selection window (e.g., "Boolean" for conditions)
        /// </summary>
        protected abstract string GetReturnTypeFilter();

        /// <summary>
        /// Gets the success message key for adding an item
        /// </summary>
        protected abstract string GetAddSuccessMessageKey();

        /// <summary>
        /// Gets the success message key for removing an item
        /// </summary>
        protected abstract string GetRemoveSuccessMessageKey();

        /// <summary>
        /// Gets the success message key for updating an item
        /// </summary>
        protected abstract string GetUpdateSuccessMessageKey();

        /// <summary>
        /// Adds a new item
        /// </summary>
        public void Add()
        {
            if (_windowData.SelectedModEvent == null) return;

            var selectWindow = new ModEventItemSelectWindow
            {
                Owner = _owner,
                ItemType = Enums.ModEventItemType.Action,
                ReturnType = GetReturnTypeFilter()
            };

            if (selectWindow.ShowDialog() == true)
            {
                var actionInfo = selectWindow.WindowData.SelectedItem;
                if (actionInfo != null)
                {
                    var newAction = actionInfo;

                    // Add SubItems as children if defined
                    AddSubItems(newAction, selectWindow.WindowData.AllItems);

                    // Add to the appropriate location in the tree
                    AddToTree(newAction);

                    _windowData.StatusMessage = MessageHelper.GetFormat(GetAddSuccessMessageKey(), actionInfo.DisplayName);
                }
            }
        }

        /// <summary>
        /// Removes an item
        /// </summary>
        public void Remove(EventActionBase item)
        {
            if (item == null || _windowData.SelectedModEvent == null) return;
            if (item.Name == Constants.EventActionRootElement.Name || item.IsHidden) return;

            if (item.Parent != null)
            {
                item.Parent.Children.Remove(item);
            }
            else
            {
                GetCollection().Remove(item);
            }
            _windowData.StatusMessage = MessageHelper.GetFormat(GetRemoveSuccessMessageKey(), item.DisplayName);
        }

        /// <summary>
        /// Updates an existing item
        /// </summary>
        public void Update(EventActionBase selectedItem)
        {
            if (selectedItem == null) return;
            if (selectedItem.Name == Constants.EventActionRootElement.Name || selectedItem.IsHidden) return;

            var selectWindow = new ModEventItemSelectWindow
            {
                Owner = _owner,
                ItemType = Enums.ModEventItemType.Action,
                ReturnType = GetReturnTypeFilter(),
                SelectedItemName = selectedItem.Name,
                ParameterValues = selectedItem.ParameterValues
            };

            if (selectWindow.ShowDialog() == true)
            {
                var newActionInfo = selectWindow.WindowData.SelectedItem;
                if (newActionInfo != null && newActionInfo.Name != selectedItem.Name)
                {
                    // Update the item properties using ObjectHelper.Map
                    ObjectHelper.Map(newActionInfo, selectedItem);

                    _windowData.StatusMessage = MessageHelper.GetFormat(GetUpdateSuccessMessageKey(), selectedItem.DisplayName);
                }
            }
        }

        /// <summary>
        /// Adds SubItems as children to an action
        /// </summary>
        private void AddSubItems(EventActionBase action, ObservableCollection<EventActionBase> allItems)
        {
            if (action.SubItems != null && action.SubItems.Count > 0)
            {
                foreach (var subItemName in action.SubItems)
                {
                    var subAction = allItems.FirstOrDefault(a => a.Name == subItemName);
                    if (subAction != null)
                    {
                        action.Children.Add(subAction);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an action to the tree at the appropriate location
        /// </summary>
        private void AddToTree(EventActionBase newAction)
        {
            var selectedItem = _treeView?.SelectedItem as EventActionBase;
            if (selectedItem != null && selectedItem.IsCanAddChild)
            {
                selectedItem.Children.Add(newAction);
            }
            else
            {
                var root = GetCollection().FirstOrDefault(c => c.Name == Constants.EventActionRootElement.Name);
                if (root != null)
                {
                    root.Children.Add(newAction);
                }
            }
        }
    }

    /// <summary>
    /// Business logic for Condition management in ProjectEditorWindow.Tab5
    /// </summary>
    public class ConditionManagementBusiness : EventActionManagementBusinessBase
    {
        public ConditionManagementBusiness(
            ProjectEditorWindowData windowData,
            Window owner,
            TreeView treeView)
            : base(windowData, owner, treeView)
        {
        }

        protected override ObservableCollection<EventActionBase> GetCollection()
        {
            return _windowData.SelectedModEvent?.Conditions;
        }

        protected override string GetReturnTypeFilter()
        {
            return "Boolean";
        }

        protected override string GetAddSuccessMessageKey()
        {
            return "Messages.Success.AddedCondition";
        }

        protected override string GetRemoveSuccessMessageKey()
        {
            return "Messages.Success.RemovedCondition";
        }

        protected override string GetUpdateSuccessMessageKey()
        {
            return "Messages.Success.UpdatedCondition";
        }
    }

    /// <summary>
    /// Business logic for Action management in ProjectEditorWindow.Tab5
    /// </summary>
    public class ActionManagementBusiness : EventActionManagementBusinessBase
    {
        public ActionManagementBusiness(
            ProjectEditorWindowData windowData,
            Window owner,
            TreeView treeView)
            : base(windowData, owner, treeView)
        {
        }

        protected override ObservableCollection<EventActionBase> GetCollection()
        {
            return _windowData.SelectedModEvent?.Actions;
        }

        protected override string GetReturnTypeFilter()
        {
            return null; // No filter for actions
        }

        protected override string GetAddSuccessMessageKey()
        {
            return "Messages.Success.AddedAction";
        }

        protected override string GetRemoveSuccessMessageKey()
        {
            return "Messages.Success.RemovedAction";
        }

        protected override string GetUpdateSuccessMessageKey()
        {
            return "Messages.Success.UpdatedAction";
        }
    }
}
