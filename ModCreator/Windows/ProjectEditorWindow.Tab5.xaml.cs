using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        private void TreeView_EventSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedEventItem = fileItem;
            }
        }

        private void CreateModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            var modPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");

            string parentPath = modPath;
            if (WindowData.SelectedEventItem != null)
            {
                parentPath = WindowData.SelectedEventItem.IsFolder
                    ? WindowData.SelectedEventItem.FullPath
                    : Path.GetDirectoryName(WindowData.SelectedEventItem.FullPath);
            }

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { WindowTitle = "Create New Folder", Label = "Folder name:", InputValue = "NewFolder" }
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
            WindowData.LoadModEventFiles();
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.CreatedModEventFolder", folderName);
            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.FolderCreated", folderName), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedEventItem == null || !WindowData.SelectedEventItem.IsFolder) return;

            var folderPath = WindowData.SelectedEventItem.FullPath;
            var folderName = WindowData.SelectedEventItem.Name;

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.FolderDoesNotExist", folderName), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                WindowData.LoadModEventFiles();
                return;
            }

            var hasContents = Directory.GetFileSystemEntries(folderPath).Length > 0;
            var warningMessage = hasContents
                ? MessageHelper.GetFormat("Messages.Confirmation.DeleteFolder", folderName)
                : MessageHelper.GetFormat("Messages.Confirmation.DeleteFolderEmpty", folderName);

            var result = MessageBox.Show(warningMessage, MessageHelper.Get("Messages.Confirmation.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Directory.Delete(folderPath, true);
                WindowData.LoadModEventFiles();
                WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedModEventFolder", folderName);
                MessageBox.Show(MessageHelper.GetFormat("Messages.Success.FolderDeleted", folderName), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CreateModEvent_Click(object sender, RoutedEventArgs e)
        {
            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = {
                    WindowTitle = MessageHelper.Get("Messages.Dialogs.CreateModEvent.Title"),
                    Label = MessageHelper.Get("Messages.Dialogs.CreateModEvent.Label"),
                    InputValue = MessageHelper.Get("Messages.Dialogs.CreateModEvent.DefaultValue")
                }
            };

            if (inputWindow.ShowDialog() != true) return;

            var className = inputWindow.WindowData.InputValue;

            if (string.IsNullOrWhiteSpace(className) || !System.Text.RegularExpressions.Regex.IsMatch(className, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.InvalidClassName"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var modPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");

            string targetPath = modPath;
            if (WindowData.SelectedEventItem != null)
            {
                targetPath = WindowData.SelectedEventItem.IsFolder
                    ? WindowData.SelectedEventItem.FullPath
                    : Path.GetDirectoryName(WindowData.SelectedEventItem.FullPath);
            }

            Directory.CreateDirectory(targetPath);

            var filePath = Path.Combine(targetPath, $"{className}.cs");

            if (File.Exists(filePath))
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.ClassNameExists", className), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var maxOrder = WindowData.EventItems.Select(x => x.GetObjectContentAs<ModEventItem>().OrderIndex).Max();

            var newEvent = new ModEventItem
            {
                OrderIndex = maxOrder + 1,
                CacheType = "Local",
                WorkOn = "Local",
                SelectedEvent = "OnTimeUpdate1000ms",
                FilePath = filePath
            };

            File.WriteAllText(filePath, WindowData.GenerateModEventCode(newEvent));
            WindowData.Project.ModEvents.Add(newEvent);
            WindowData.LoadModEventFiles();
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.CreatedModEvent", className);

            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.ModEventCreated", className), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloneModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var newClassName = $"{WindowData.SelectedModEvent.FileName}_Copy";

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { WindowTitle = "Clone ModEvent", Label = "New class name:", InputValue = newClassName }
            };

            if (inputWindow.ShowDialog() != true) return;

            newClassName = inputWindow.WindowData.InputValue;

            if (string.IsNullOrWhiteSpace(newClassName) || !System.Text.RegularExpressions.Regex.IsMatch(newClassName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show("Invalid class name!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var modPath = Path.GetDirectoryName(WindowData.SelectedModEvent.FilePath);
            var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

            if (File.Exists(newFilePath))
            {
                MessageBox.Show($"A ModEvent with name '{newClassName}' already exists!", MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var clonedEvent = new ModEventItem
            {
                OrderIndex = WindowData.SelectedModEvent.OrderIndex,
                CacheType = WindowData.SelectedModEvent.CacheType,
                WorkOn = WindowData.SelectedModEvent.WorkOn,
                SelectedEvent = WindowData.SelectedModEvent.SelectedEvent,
                FilePath = newFilePath
            };

            foreach (var condition in WindowData.SelectedModEvent.Conditions)
            {
                clonedEvent.Conditions.Add(new EventActionBase
                {
                    Name = condition.Name,
                    Category = condition.Category,
                    DisplayName = condition.DisplayName,
                    Description = condition.Description,
                    Code = condition.Code
                });
            }

            foreach (var action in WindowData.SelectedModEvent.Actions)
            {
                clonedEvent.Actions.Add(new EventActionBase
                {
                    Name = action.Name,
                    Category = action.Category,
                    DisplayName = action.DisplayName,
                    Description = action.Description,
                    Code = action.Code
                });
            }

            File.WriteAllText(newFilePath, WindowData.GenerateModEventCode(clonedEvent));
            WindowData.LoadModEventFiles();
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.ClonedModEvent", newClassName);

            MessageBox.Show(MessageHelper.Get("Messages.Success.ModEventCloned"), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RenameModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var oldClassName = WindowData.SelectedModEvent.FileName;

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { WindowTitle = "Rename ModEvent", Label = "New class name:", InputValue = oldClassName }
            };

            if (inputWindow.ShowDialog() != true) return;

            var newClassName = inputWindow.WindowData.InputValue;

            if (newClassName == oldClassName) return;

            if (string.IsNullOrWhiteSpace(newClassName) || !System.Text.RegularExpressions.Regex.IsMatch(newClassName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.InvalidClassName"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var oldFilePath = WindowData.SelectedModEvent.FilePath;
            var modPath = Path.GetDirectoryName(oldFilePath);
            var newFilePath = Path.Combine(modPath, $"{newClassName}.cs");

            if (File.Exists(newFilePath))
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.ClassNameExists", newClassName), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WindowData.SelectedModEvent.FilePath = newFilePath;
            WindowData.SaveModEvent();

            if (File.Exists(oldFilePath))
                File.Delete(oldFilePath);

            WindowData.LoadModEventFiles();
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.RenamedModEvent", oldClassName, newClassName);

            MessageBox.Show(MessageHelper.Get("Messages.Success.ModEventRenamed"), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteModEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var result = MessageBox.Show(MessageHelper.GetFormat("Messages.Confirmation.DeleteModEvent", WindowData.SelectedModEvent.FileName), MessageHelper.Get("Messages.Confirmation.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var filePath = WindowData.SelectedModEvent.FilePath;
            var fileName = WindowData.SelectedModEvent.FileName;
            if (File.Exists(filePath))
                File.Delete(filePath);

            WindowData.LoadModEventFiles();
            WindowData.SelectedModEvent = null;
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedModEvent", fileName);

            MessageBox.Show(MessageHelper.Get("Messages.Success.ModEventDeleted"), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveModEvent_Click(object sender, RoutedEventArgs e)
        {
            WindowData.SaveModEvent();
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEvent", WindowData.SelectedModEvent?.FileName);
            MessageBox.Show(MessageHelper.Get("Messages.Success.ModEventSaved"), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var selectWindow = new ModEventItemSelectWindow { Owner = this, ItemType = Enums.ModEventItemType.Event };

            if (selectWindow.ShowDialog() == true)
            {
                var selectedItem = selectWindow.WindowData.SelectedItem;
                if (selectedItem != null)
                {
                    WindowData.SelectedModEvent.SelectedEvent = selectedItem.Name;
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.AddedEvent", selectedItem.DisplayName);
                }
            }
        }

        private void AddCondition_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var selectWindow = new ModEventItemSelectWindow { Owner = this, ItemType = Enums.ModEventItemType.Action, ReturnType = "Boolean" };

            if (selectWindow.ShowDialog() == true)
            {
                var actionInfo = selectWindow.WindowData.SelectedItem;
                if (actionInfo != null)
                {
                    var newAction = actionInfo;

                    // Add SubItems as children if defined
                    if (newAction.SubItems != null && newAction.SubItems.Count > 0)
                    {
                        var allActions = selectWindow.WindowData.AllItems;
                        foreach (var subItemName in newAction.SubItems)
                        {
                            var subAction = allActions.FirstOrDefault(a => a.Name == subItemName);
                            if (subAction != null)
                            {
                                newAction.Children.Add(subAction);
                            }
                        }
                    }

                    var selectedItem = tvConditions.SelectedItem as EventActionBase;
                    if (selectedItem != null && selectedItem.IsCanAddChild)
                    {
                        selectedItem.Children.Add(newAction);
                    }
                    else
                    {
                        var root = WindowData.SelectedModEvent.Conditions.FirstOrDefault(c => c.Name == Constants.EventActionRootElement.Name);
                        if (root != null)
                        {
                            root.Children.Add(newAction);
                        }
                    }

                    newAction.RefreshDisplayName();
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.AddedCondition", actionInfo.DisplayName);
                }
            }
        }

        private void RemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            var action = (sender as Button)?.Tag as EventActionBase;
            RemoveConditionItem(action);
        }

        private void RemoveConditionItem(EventActionBase item)
        {
            if (item == null || WindowData.SelectedModEvent == null) return;
            if (item.Name == Constants.EventActionRootElement.Name || item.IsHidden) return;

            if (item.Parent != null)
            {
                item.Parent.Children.Remove(item);
            }
            else
            {
                WindowData.SelectedModEvent.Conditions.Remove(item);
            }
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.RemovedCondition", item.DisplayName);
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedModEvent == null) return;

            var selectWindow = new ModEventItemSelectWindow { Owner = this, ItemType = Enums.ModEventItemType.Action };

            if (selectWindow.ShowDialog() == true)
            {
                var actionInfo = selectWindow.WindowData.SelectedItem;
                if (actionInfo != null)
                {
                    var newAction = actionInfo;

                    // Add SubItems as children if defined
                    if (newAction.SubItems != null && newAction.SubItems.Count > 0)
                    {
                        var allActions = selectWindow.WindowData.AllItems;
                        foreach (var subItemName in newAction.SubItems)
                        {
                            var subAction = allActions.FirstOrDefault(a => a.Name == subItemName);
                            if (subAction != null)
                            {
                                newAction.Children.Add(subAction);
                            }
                        }
                    }

                    var selectedItem = tvActions.SelectedItem as EventActionBase;
                    if (selectedItem != null && selectedItem.IsCanAddChild)
                    {
                        selectedItem.Children.Add(newAction);
                    }
                    else
                    {
                        var root = WindowData.SelectedModEvent.Actions.FirstOrDefault(a => a.Name == Constants.EventActionRootElement.Name);
                        if (root != null)
                        {
                            root.Children.Add(newAction);
                        }
                    }

                    newAction.RefreshDisplayName();
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.AddedAction", actionInfo.DisplayName);
                }
            }
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            var action = (sender as Button)?.Tag as EventActionBase;
            RemoveActionItem(action);
        }

        private void RemoveActionItem(EventActionBase item)
        {
            if (item == null || WindowData.SelectedModEvent == null) return;
            if (item.Name == Constants.EventActionRootElement.Name || item.IsHidden) return;

            if (item.Parent != null)
            {
                item.Parent.Children.Remove(item);
            }
            else
            {
                WindowData.SelectedModEvent.Actions.Remove(item);
            }
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.RemovedAction", item.DisplayName);
        }

        private void Conditions_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = tvConditions.SelectedItem as EventActionBase;
            if (selectedItem != null && selectedItem.Name != Constants.EventActionRootElement.Name && !selectedItem.IsHidden)
            {
                var selectWindow = new ModEventItemSelectWindow
                {
                    Owner = this,
                    ItemType = Enums.ModEventItemType.Action,
                    ReturnType = "Boolean",
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

                        WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.UpdatedCondition", selectedItem.DisplayName);
                    }
                }
            }
        }

        private void Conditions_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                var selectedItem = tvConditions.SelectedItem as EventActionBase;
                if (selectedItem != null)
                {
                    RemoveConditionItem(selectedItem);
                    e.Handled = true;
                }
            }
        }

        private void Actions_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = tvActions.SelectedItem as EventActionBase;
            if (selectedItem != null && selectedItem.Name != Constants.EventActionRootElement.Name && !selectedItem.IsHidden)
            {
                var selectWindow = new ModEventItemSelectWindow
                {
                    Owner = this,
                    ItemType = Enums.ModEventItemType.Action,
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

                        WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.UpdatedAction", selectedItem.DisplayName);
                    }
                }
            }
        }

        private void Actions_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                var selectedItem = tvActions.SelectedItem as EventActionBase;
                if (selectedItem != null)
                {
                    RemoveActionItem(selectedItem);
                    e.Handled = true;
                }
            }
        }

        private void OpenModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.Project == null) return;

            var modPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");
            Directory.CreateDirectory(modPath);
            System.Diagnostics.Process.Start("explorer.exe", modPath);
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.OpenedModEventFolder", modPath);
        }

        [SupportedOSPlatform("windows6.1")]
        private void ToggleGuiMode_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.SelectedModEvent?.IsCodeModeOnly == true)
            {
                MessageBox.Show(
                    MessageHelper.Get("Messages.Warning.CannotSwitchToGuiMode"),
                    MessageHelper.Get("Messages.Warning.Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            WindowData.StatusMessage = MessageHelper.Get("Messages.Success.SwitchedToGuiMode");
        }

        [SupportedOSPlatform("windows6.1")]
        private void ToggleCodeMode_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.SelectedModEvent == null) return;

            if (!WindowData.SelectedModEvent.IsCodeModeOnly)
            {
                var result = MessageBox.Show(
                    MessageHelper.Get("Messages.Warning.SwitchToCodeModeWarning"),
                    MessageHelper.Get("Messages.Warning.Title"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                WindowData.SelectedModEvent.IsCodeModeOnly = true;
                WindowData.SelectedModEvent.SelectedEvent = string.Empty;
                WindowData.SelectedModEvent.Conditions.Clear();
                WindowData.SelectedModEvent.Actions.Clear();
            }

            WindowData.StatusMessage = MessageHelper.Get("Messages.Success.SwitchedToCodeMode");
        }

        [SupportedOSPlatform("windows6.1")]
        private void SetupEventSourceEditorBinding()
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null || editor.Tag != null) return; // Already setup

            editor.Tag = "setup"; // Mark as setup

            // Load C# syntax highlighting
            AvalonHelper.LoadCSharpSyntaxHighlighting(editor);

            // Subscribe to property changes
            WindowData.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WindowData.EventSourceContent))
                {
                    if (editor.Text != WindowData.EventSourceContent)
                    {
                        editor.Text = WindowData.EventSourceContent ?? string.Empty;
                    }
                }
            };

            // Subscribe to editor changes
            editor.TextChanged += (s, e) =>
            {
                if (WindowData != null && editor.Text != WindowData.EventSourceContent)
                {
                    WindowData.EventSourceContent = editor.Text;
                }
            };
        }

        // Drag/drop for conditions
        private EventActionBase _draggedCondition;

        private void Conditions_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Don't start drag-drop if clicking on a Button
            if (e.OriginalSource is DependencyObject dep)
            {
                var button = FindAncestor<Button>(dep);
                if (button != null) return;
            }

            var treeView = sender as System.Windows.Controls.TreeView;
            if (treeView == null) return;

            var item = ItemsControl.ContainerFromElement(treeView, e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                _draggedCondition = item.Header as EventActionBase;
                if (_draggedCondition != null)
                {
                    DragDrop.DoDragDrop(item, _draggedCondition, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                    return ancestor;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void Conditions_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (_draggedCondition == null || WindowData.SelectedModEvent == null) return;

            var treeView = sender as System.Windows.Controls.TreeView;
            if (treeView == null) return;

            var targetItem = ItemsControl.ContainerFromElement(treeView, e.OriginalSource as DependencyObject) as TreeViewItem;
            var targetAction = targetItem?.Header as EventActionBase;

            if (targetAction != null && targetAction != _draggedCondition)
            {
                var conditions = WindowData.SelectedModEvent.Conditions;
                int oldIndex = conditions.IndexOf(_draggedCondition);
                int newIndex = conditions.IndexOf(targetAction);

                if (oldIndex >= 0 && newIndex >= 0)
                {
                    conditions.RemoveAt(oldIndex);
                    conditions.Insert(newIndex, _draggedCondition);
                }
            }

            _draggedCondition = null;
        }

        // Drag/drop for actions
        private EventActionBase _draggedAction;

        private void Actions_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Don't start drag-drop if clicking on a Button
            if (e.OriginalSource is DependencyObject dep)
            {
                var button = FindAncestor<Button>(dep);
                if (button != null) return;
            }

            var treeView = sender as System.Windows.Controls.TreeView;
            if (treeView == null) return;

            var item = ItemsControl.ContainerFromElement(treeView, e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                _draggedAction = item.Header as EventActionBase;
                if (_draggedAction != null)
                {
                    DragDrop.DoDragDrop(item, _draggedAction, System.Windows.DragDropEffects.Move);
                }
            }
        }

        private void Actions_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (_draggedAction == null || WindowData.SelectedModEvent == null) return;

            var treeView = sender as System.Windows.Controls.TreeView;
            if (treeView == null) return;

            var targetItem = ItemsControl.ContainerFromElement(treeView, e.OriginalSource as DependencyObject) as TreeViewItem;
            var targetAction = targetItem?.Header as EventActionBase;

            if (targetAction != null && targetAction != _draggedAction)
            {
                var actions = WindowData.SelectedModEvent.Actions;
                int oldIndex = actions.IndexOf(_draggedAction);
                int newIndex = actions.IndexOf(targetAction);

                if (oldIndex >= 0 && newIndex >= 0)
                {
                    actions.RemoveAt(oldIndex);
                    actions.Insert(newIndex, _draggedAction);
                }
            }

            _draggedAction = null;
        }

        // Number validation
        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        // Event mode selection changed
        [SupportedOSPlatform("windows6.1")]
        private void EventMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox comboBox) || WindowData?.SelectedModEvent == null) return;

            var mode = comboBox.SelectedItem as Enums.EventMode?;
            var grpEventSelection = this.FindName("grpEventSelection") as GroupBox;
            var isModEvent = mode == Enums.EventMode.ModEvent;

            if (grpEventSelection != null)
                grpEventSelection.Visibility = isModEvent ? Visibility.Visible : Visibility.Collapsed;
        }

        // Event editor search/replace handlers
        [SupportedOSPlatform("windows6.1")]
        private void ReplaceInEventEditor_Click(object sender, RoutedEventArgs e)
        {
            var replacePanel = this.FindName("eventReplacePanel") as Border;
            if (replacePanel == null) return;

            replacePanel.Visibility = replacePanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            if (replacePanel.Visibility == Visibility.Visible)
                (this.FindName("txtEventFindText") as TextBox)?.Focus();
        }

        [SupportedOSPlatform("windows6.1")]
        private void TxtEventFindText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtFind = sender as TextBox;
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;

            if (editor == null || txtFind == null || string.IsNullOrEmpty(txtFind.Text)) return;

            var index = editor.Text.IndexOf(txtFind.Text, 0, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                editor.Select(index, txtFind.Text.Length);
                editor.CaretOffset = index + txtFind.Text.Length;
                editor.ScrollToLine(editor.Document.GetLineByOffset(index).LineNumber);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void TxtEventFindText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                EventFindNext_Click(sender, e);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void EventFindNext_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as TextBox;

            if (editor == null || txtFind == null || string.IsNullOrEmpty(txtFind.Text)) return;

            var searchText = txtFind.Text;
            var index = editor.Text.IndexOf(searchText, editor.CaretOffset, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
                index = editor.Text.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                editor.Select(index, searchText.Length);
                editor.Select(index, searchText.Length);
                editor.ScrollTo(editor.Document.GetLineByOffset(index).LineNumber, 0);
            }
            else
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Info.CannotFind", searchText), MessageHelper.Get("Messages.Info.Find"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void EventReplaceOne_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as TextBox;
            var txtReplace = this.FindName("txtEventReplaceText") as TextBox;

            if (editor == null || txtFind == null || txtReplace == null || string.IsNullOrEmpty(txtFind.Text)) return;

            var searchText = txtFind.Text;
            var replaceText = txtReplace.Text;

            if (editor.SelectedText.Equals(searchText, StringComparison.OrdinalIgnoreCase))
            {
                var offset = editor.SelectionStart;
                editor.Document.Replace(offset, editor.SelectionLength, replaceText);
                editor.CaretOffset = offset + replaceText.Length;
            }

            EventFindNext_Click(sender, e);
        }

        [SupportedOSPlatform("windows6.1")]
        private void EventReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.FindName("txtEventSourceEditor") as ICSharpCode.AvalonEdit.TextEditor;
            var txtFind = this.FindName("txtEventFindText") as TextBox;
            var txtReplace = this.FindName("txtEventReplaceText") as TextBox;

            if (editor == null || txtFind == null || txtReplace == null || string.IsNullOrEmpty(txtFind.Text)) return;

            var searchText = txtFind.Text;
            var replaceText = txtReplace.Text;
            var text = editor.Text;
            var count = 0;
            var index = 0;
            var offset = 0;

            while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                editor.Document.Replace(index + offset, searchText.Length, replaceText);
                offset += replaceText.Length - searchText.Length;
                index += searchText.Length;
                count++;
            }

            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.ReplaceAll", count), MessageHelper.Get("Messages.Info.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseEventReplacePanel_Click(object sender, RoutedEventArgs e)
        {
            (this.FindName("eventReplacePanel") as Border)?.SetValue(VisibilityProperty, Visibility.Collapsed);
        }
    }
}