using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ModCreator.Controls
{
    public partial class MultiSelectComboBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler DropDownOpened;

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<dynamic>), typeof(MultiSelectComboBox),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(string), typeof(MultiSelectComboBox),
                new PropertyMetadata(string.Empty, OnSelectedValueChanged));

        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register("Separator", typeof(string), typeof(MultiSelectComboBox),
                new PropertyMetadata(","));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(MultiSelectComboBox),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(MultiSelectComboBox),
                new PropertyMetadata(false));

        private string _displayText = string.Empty;
        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));

                    // If user manually types, update SelectedValue and clear selections
                    if (!_isUpdating && !string.IsNullOrEmpty(value))
                    {
                        _isUpdating = true;
                        SelectedValue = value;
                        if (ItemsSource != null)
                        {
                            foreach (var item in ItemsSource)
                            {
                                item.IsSelected = false;
                            }
                        }
                        _isUpdating = false;
                    }
                }
            }
        }

        public IEnumerable<dynamic> ItemsSource
        {
            get => (IEnumerable<dynamic>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string SelectedValue
        {
            get => (string)GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        public string Separator
        {
            get => (string)GetValue(SeparatorProperty);
            set => SetValue(SeparatorProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty) && !(bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsEditableProperty, value);
        }

        private bool _isUpdating = false;

        public MultiSelectComboBox()
        {
            InitializeComponent();
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control)
            {
                control.MainComboBox.ItemsSource = e.NewValue as IEnumerable<dynamic>;
                control.UpdateSelectionFromValue();
            }
        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control && !control._isUpdating)
            {
                control.UpdateSelectionFromValue();
            }
        }

        private void UpdateSelectionFromValue()
        {
            if (ItemsSource == null || _isUpdating)
                return;

            _isUpdating = true;

            var separator = string.IsNullOrEmpty(Separator) ? "," : Separator;
            var selectedValues = string.IsNullOrEmpty(SelectedValue)
                ? new HashSet<string>()
                : new HashSet<string>(SelectedValue.Split(new string[] { separator }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim()));

            var selectedDisplayNames = new List<string>();
            foreach (var item in ItemsSource)
            {
                item.IsSelected = selectedValues.Contains(item.Value);
                if (item.IsSelected)
                {
                    selectedDisplayNames.Add(item.DisplayName);
                }
            }

            DisplayText = selectedDisplayNames.Count > 0 
                ? string.Join(separator, selectedDisplayNames) 
                : string.Empty;

            _isUpdating = false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdating)
                return;

            _isUpdating = true;

            var separator = string.IsNullOrEmpty(Separator) ? "," : Separator;
            var selectedItems = ItemsSource?.Where(i => i.IsSelected).ToList();
            
            SelectedValue = selectedItems != null && selectedItems.Count > 0
                ? string.Join(separator, selectedItems.Select(i => i.Value))
                : string.Empty;

            DisplayText = selectedItems != null && selectedItems.Count > 0
                ? string.Join(separator, selectedItems.Select(i => i.DisplayName))
                : string.Empty;

            _isUpdating = false;
        }

        private void MainComboBox_DropDownOpened(object sender, EventArgs e)
        {
            DropDownOpened?.Invoke(this, e);
            //UpdateSelectionFromValue();
        }
    }
}
