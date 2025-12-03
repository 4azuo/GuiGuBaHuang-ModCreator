using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.Windows;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ModCreator.Controls
{
    public class ParameterizedTextBlock : TextBlock
    {
        private static readonly Regex ParameterPlaceholderRegex = new(@"\{(\d+)\}");

        public static readonly DependencyProperty ItemProperty =
         DependencyProperty.Register(nameof(Item), typeof(EventActionBase), typeof(ParameterizedTextBlock), new PropertyMetadata(null, OnItemChanged));

        public static readonly DependencyProperty AllVariablesProperty =
               DependencyProperty.Register(nameof(AllVariables), typeof(List<GlobalVariable>), typeof(ParameterizedTextBlock), new PropertyMetadata(null));

        public EventActionBase Item
        {
            get => (EventActionBase)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public List<GlobalVariable> AllVariables
        {
            get => (List<GlobalVariable>)GetValue(AllVariablesProperty);
            set => SetValue(AllVariablesProperty, value);
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ParameterizedTextBlock textBlock && e.NewValue is EventActionBase item)
            {
                textBlock.UpdateText(item);
            }
        }

        private void UpdateText(EventActionBase item)
        {
            Inlines.Clear();

            if (item == null || string.IsNullOrEmpty(item.DisplayName))
                return;

            var displayName = item.DisplayName;
            var matches = ParameterPlaceholderRegex.Matches(displayName);

            if (matches.Count == 0)
            {
                Inlines.Add(new Run(displayName));
                return;
            }

            int lastIndex = 0;
            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    Inlines.Add(new Run(displayName.Substring(lastIndex, match.Index - lastIndex)));
                }

                if (int.TryParse(match.Groups[1].Value, out int paramIndex) && paramIndex < item.Parameters.Count)
                {
                    var parameter = item.Parameters[paramIndex];

                    string displayText;
                    if (item.ParameterValues.ContainsKey(paramIndex) && item.ParameterValues[paramIndex] != null)
                    {
                        displayText = DisplayNameHelper.BuildNestedDisplayName(item.ParameterValues[paramIndex]);
                    }
                    else
                    {
                        displayText = $"{{{paramIndex}}}";
                    }

                    var hyperlink = new Hyperlink(new Run(displayText))
                    {
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 102, 204)),
                        TextDecorations = System.Windows.TextDecorations.Underline,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        ToolTip = $"Click to select {parameter.Name} ({parameter.Type})"
                    };

                    hyperlink.Click += (s, e) => OnHyperlinkClick(item, paramIndex, parameter);
                    Inlines.Add(hyperlink);
                }
                else
                {
                    Inlines.Add(new Run(match.Value));
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < displayName.Length)
            {
                Inlines.Add(new Run(displayName.Substring(lastIndex)));
            }
        }

        private void OnHyperlinkClick(EventActionBase item, int paramIndex, ParameterInfo parameter)
        {
            var window = Window.GetWindow(this);
            if (window == null)
                return;

            var vars = AllVariables ?? [];
            if (window is ModEventItemSelectWindow parentWindow && parentWindow.WindowData?.AllVariables != null)
            {
                vars = parentWindow.WindowData.AllVariables;
            }

            var p = item.ParameterValues.ContainsKey(paramIndex) ? item.ParameterValues[paramIndex] : null;
            var selectWindow = new ModEventItemSelectWindow
            {
                Owner = window,
                ItemType = ModEventItemType.Action,
                ReturnType = parameter.Type,
                AllVariables = vars,
                ParameterValues = p != null && p.SelectType == ModEventSelectType.EventAction ? p.SelectedEventAction.ParameterValues : [],
                SelectedItemName = p?.Name,
                ShowVariablesSection = true
            };

            if (selectWindow.ShowDialog() == true)
            {
                if (selectWindow.WindowData.SelectType == ModEventSelectType.EventAction && selectWindow.WindowData.SelectedItem != null)
                {
                    item.ParameterValues[paramIndex] = new ModEventItemSelectValue(selectWindow.WindowData.SelectedItem);
                }
                else if (selectWindow.WindowData.SelectType == ModEventSelectType.Variable && selectWindow.WindowData.SelectedVariable != null)
                {
                    item.ParameterValues[paramIndex] = new ModEventItemSelectValue(selectWindow.WindowData.SelectedVariable);
                }
                else if (selectWindow.WindowData.SelectType == ModEventSelectType.OptionalValue && selectWindow.WindowData.HasOptionalValue)
                {
                    item.ParameterValues[paramIndex] = new ModEventItemSelectValue(selectWindow.WindowData.OptionalValue, parameter.Type);
                }

                item.RefreshDisplayName();
                UpdateText(item);
            }
        }
    }
}