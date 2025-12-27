using ModCreator.Helpers;
using ModCreator.WindowData;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ModCreator.Windows
{
    public partial class PatternRefDocsWindow : CWindow<PatternRefDocsWindowData>
    {
        public string FilePath { get; set; }
        private int _lastSearchOffset = 0;

        public override PatternRefDocsWindowData InitData(CancelEventArgs e)
        {
            var data = base.InitData(e);

            Loaded += (s, ev) =>
            {
                data.FilePath = FilePath;

                // Setup JSON syntax highlighting
                AvalonHelper.LoadJsonSyntaxHighlighting(txtEditor);

                // Load file content
                if (!string.IsNullOrEmpty(data.FilePath) && File.Exists(data.FilePath))
                {
                    data.FileName = Path.GetFileName(data.FilePath);
                    data.FileContent = File.ReadAllText(data.FilePath);
                    txtEditor.Text = data.FileContent;
                }

                // Setup Ctrl+F shortcut
                PreviewKeyDown += Window_PreviewKeyDown;
            };

            return data;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ToggleSearchPanel();
                e.Handled = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ToggleSearchPanel();
        }

        private void ToggleSearchPanel()
        {
            if (searchPanel.Visibility == Visibility.Collapsed)
            {
                searchPanel.Visibility = Visibility.Visible;
                txtSearchBox.Focus();
                txtSearchBox.SelectAll();
            }
            else
            {
                searchPanel.Visibility = Visibility.Collapsed;
                txtEditor.Focus();
            }
        }

        private void TxtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                    FindPrevious();
                else
                    FindNext();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CloseSearch();
                e.Handled = true;
            }
        }

        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            FindNext();
        }

        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            FindPrevious();
        }

        private void CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            CloseSearch();
        }

        private void CloseSearch()
        {
            searchPanel.Visibility = Visibility.Collapsed;
            txtEditor.Focus();
            _lastSearchOffset = 0;
        }

        private void FindNext()
        {
            if (string.IsNullOrEmpty(txtSearchBox.Text))
                return;

            string searchText = txtSearchBox.Text;
            string documentText = txtEditor.Text;
            bool matchCase = chkMatchCase.IsChecked == true;

            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int startIndex = txtEditor.CaretOffset;
            int foundIndex = documentText.IndexOf(searchText, startIndex, comparison);

            if (foundIndex == -1 && startIndex > 0)
            {
                // Wrap around to beginning
                foundIndex = documentText.IndexOf(searchText, 0, comparison);
            }

            if (foundIndex >= 0)
            {
                txtEditor.Select(foundIndex, searchText.Length);
                txtEditor.CaretOffset = foundIndex + searchText.Length;
                txtEditor.ScrollToLine(txtEditor.Document.GetLineByOffset(foundIndex).LineNumber);
                _lastSearchOffset = foundIndex;
            }
            else
            {
                MessageBox.Show($"Cannot find \"{searchText}\"", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FindPrevious()
        {
            if (string.IsNullOrEmpty(txtSearchBox.Text))
                return;

            string searchText = txtSearchBox.Text;
            string documentText = txtEditor.Text;
            bool matchCase = chkMatchCase.IsChecked == true;

            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int startIndex = txtEditor.SelectionStart - 1;
            if (startIndex < 0) startIndex = documentText.Length - 1;

            int foundIndex = documentText.LastIndexOf(searchText, startIndex, comparison);

            if (foundIndex == -1 && startIndex < documentText.Length - 1)
            {
                // Wrap around to end
                foundIndex = documentText.LastIndexOf(searchText, documentText.Length - 1, comparison);
            }

            if (foundIndex >= 0)
            {
                txtEditor.Select(foundIndex, searchText.Length);
                txtEditor.CaretOffset = foundIndex + searchText.Length;
                txtEditor.ScrollToLine(txtEditor.Document.GetLineByOffset(foundIndex).LineNumber);
                _lastSearchOffset = foundIndex;
            }
            else
            {
                MessageBox.Show($"Cannot find \"{searchText}\"", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
