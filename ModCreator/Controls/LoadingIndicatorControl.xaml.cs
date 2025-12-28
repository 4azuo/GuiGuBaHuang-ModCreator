using System.Windows;
using System.Windows.Controls;

namespace ModCreator.Controls
{
    public partial class LoadingIndicatorControl : UserControl
    {
        public static readonly DependencyProperty LoadingTextProperty =
            DependencyProperty.Register(nameof(LoadingText), typeof(string), typeof(LoadingIndicatorControl),
                new PropertyMetadata("Loading..."));

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(LoadingIndicatorControl),
                new PropertyMetadata(48.0, OnIconSizeChanged));

        public static readonly DependencyProperty HalfIconSizeProperty =
            DependencyProperty.Register(nameof(HalfIconSize), typeof(double), typeof(LoadingIndicatorControl),
                new PropertyMetadata(24.0));

        public string LoadingText
        {
            get => (string)GetValue(LoadingTextProperty);
            set => SetValue(LoadingTextProperty, value);
        }

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public double HalfIconSize
        {
            get => (double)GetValue(HalfIconSizeProperty);
            private set => SetValue(HalfIconSizeProperty, value);
        }

        public LoadingIndicatorControl()
        {
            InitializeComponent();
        }

        private static void OnIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingIndicatorControl control)
            {
                control.HalfIconSize = (double)e.NewValue / 2;
            }
        }
    }
}
