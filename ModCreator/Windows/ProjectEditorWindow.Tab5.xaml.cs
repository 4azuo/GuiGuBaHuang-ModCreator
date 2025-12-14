using ModCreator.Businesses;
using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.Collections.ObjectModel;
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
        // Business logic handlers
        private ConditionsDragDropBusiness _conditionsDragDropBusiness;
        private ActionsDragDropBusiness _actionsDragDropBusiness;
        private ModEventFileManagementBusiness _modEventFileManagementBusiness;
        private ConditionManagementBusiness _conditionManagementBusiness;
        private ActionManagementBusiness _actionManagementBusiness;
        
        private void TreeView_EventSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedEventItem = fileItem;
            }
        }

        private void CreateModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.CreateModEventFolder();
        }

        private void DeleteModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.DeleteModEventFolder();
        }

        private void CreateModEvent_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.CreateModEvent();
        }

        private void CloneModEvent_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.CloneModEvent();
        }

        private void RenameModEvent_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.RenameModEvent();
        }

        private void DeleteModEvent_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.DeleteModEvent();
        }

        private void SaveModEvent_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.SaveModEvent();
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
            _conditionManagementBusiness?.Add();
        }

        private void RemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            var action = (sender as Button)?.Tag as EventActionBase;
            _conditionManagementBusiness?.Remove(action);
        }

        private void RemoveConditionItem(EventActionBase item)
        {
            _conditionManagementBusiness?.Remove(item);
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            _actionManagementBusiness?.Add();
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            var action = (sender as Button)?.Tag as EventActionBase;
            _actionManagementBusiness?.Remove(action);
        }

        private void RemoveActionItem(EventActionBase item)
        {
            _actionManagementBusiness?.Remove(item);
        }

        private void Conditions_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = tvConditions.SelectedItem as EventActionBase;
            if (selectedItem != null)
            {
                _conditionManagementBusiness?.Update(selectedItem);
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
            if (selectedItem != null)
            {
                _actionManagementBusiness?.Update(selectedItem);
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

        #region Drag/Drop Event Handlers (Delegated to Business)

        private void Conditions_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _conditionsDragDropBusiness?.OnPreviewMouseLeftButtonDown(e);
        }

        private void Conditions_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _conditionsDragDropBusiness?.OnMouseMove(e);
        }

        private void Conditions_DragOver(object sender, DragEventArgs e)
        {
            _conditionsDragDropBusiness?.OnDragOver(e);
        }

        private void Conditions_Drop(object sender, DragEventArgs e)
        {
            _conditionsDragDropBusiness?.OnDrop(e);
        }

        private void Actions_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _actionsDragDropBusiness?.OnPreviewMouseLeftButtonDown(e);
        }

        private void Actions_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _actionsDragDropBusiness?.OnMouseMove(e);
        }

        private void Actions_DragOver(object sender, DragEventArgs e)
        {
            _actionsDragDropBusiness?.OnDragOver(e);
        }

        private void Actions_Drop(object sender, DragEventArgs e)
        {
            _actionsDragDropBusiness?.OnDrop(e);
        }

        #endregion

        private void OpenModEventFolder_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.OpenModEventFolder();
        }

        [SupportedOSPlatform("windows6.1")]
        private void ToggleCodeMode_Click(object sender, RoutedEventArgs e)
        {
            _modEventFileManagementBusiness?.SwitchToCodeMode();
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