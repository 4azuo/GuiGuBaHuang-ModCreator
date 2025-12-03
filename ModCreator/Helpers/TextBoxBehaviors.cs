using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ModCreator.Helpers
{
    public static class TextBoxBehaviors
    {
        // Placeholder attached property
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxBehaviors),
                new PropertyMetadata(string.Empty));

        public static string GetPlaceholder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderProperty);
        }

        public static void SetPlaceholder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }

        // Clear button attached property
        public static readonly DependencyProperty EnableClearButtonProperty =
            DependencyProperty.RegisterAttached(
                "EnableClearButton",
                typeof(bool),
                typeof(TextBoxBehaviors),
                new PropertyMetadata(false, OnEnableClearButtonChanged));

        public static bool GetEnableClearButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableClearButtonProperty);
        }

        public static void SetEnableClearButton(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableClearButtonProperty, value);
        }

        private static void OnEnableClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded -= TextBox_Loaded;
                if ((bool)e.NewValue)
                {
                    textBox.Loaded += TextBox_Loaded;
                }
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var clearButton = FindVisualChild<Button>(textBox, "ClearButton");
                if (clearButton != null)
                {
                    clearButton.Click -= ClearButton_Click;
                    clearButton.Click += ClearButton_Click;
                }
            }
        }

        private static void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TextBox textBox)
            {
                textBox.Clear();
                textBox.Focus();
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild && (string.IsNullOrEmpty(name) || 
                    (child is FrameworkElement fe && fe.Name == name)))
                {
                    return typedChild;
                }

                var childOfChild = FindVisualChild<T>(child, name);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
