using ModCreator.Enums;
using ModCreator.Models;
using ModCreator.WindowData;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ModCreator.Windows
{
    public partial class ModEventItemSelectWindow : CWindow<ModEventItemSelectWindowData>
    {
        private static int _windowCount = 0;
        private const int OffsetIncrement = 30;

        public ModEventItemType ItemType { get; set; }
        public string ReturnType { get; set; } = string.Empty;
        public string SelectedItemName { get; set; } = string.Empty;
        public List<GlobalVariable> AllVariables { get; set; } = [];
        public Dictionary<int, ModEventItemSelectValue> ParameterValues { get; set; } = [];
        public bool ShowVariablesSection { get; set; } = false;
        public bool ShowOptionalValueSection { get; set; } = false;

        public override ModEventItemSelectWindowData InitData(CancelEventArgs e)
        {
            var data = new ModEventItemSelectWindowData();

            Loaded += (s, ev) =>
            {
                _windowCount++;
                var offset = (_windowCount - 1) * OffsetIncrement;
                Left += offset;
                Top += offset;

                data.ShowVariablesSection = ShowVariablesSection;
                data.ShowOptionalValueSection = ShowOptionalValueSection;
                data.Initialize(ItemType, ReturnType, SelectedItemName, AllVariables, ParameterValues);
            };

            Closed += (s, e) =>
            {
                _windowCount--;
                if (_windowCount < 0) _windowCount = 0;
            };
            return data;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedItem != null)
            {
                WindowData.SelectType = ModEventSelectType.EventAction;
                DialogResult = true;
                Close();
            }
            else if (WindowData.SelectedVariable != null)
            {
                WindowData.SelectType = ModEventSelectType.Variable;
                DialogResult = true;
                Close();
            }
            else if (WindowData.HasOptionalValue)
            {
                WindowData.SelectType = ModEventSelectType.OptionalValue;
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowData.SelectedItem != null)
            {
                WindowData.SelectType = ModEventSelectType.EventAction;
                DialogResult = true;
                Close();
            }
        }

        private void VariablesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowData.SelectedVariable != null)
            {
                WindowData.SelectType = ModEventSelectType.Variable;
                DialogResult = true;
                Close();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            WindowData.ClearSelection();
        }
    }
}