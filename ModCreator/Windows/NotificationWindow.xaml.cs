using ModCreator.WindowData;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class NotificationWindow : CWindow<NotificationWindowData>
    {
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public override NotificationWindowData InitData(CancelEventArgs e)
        {
            var data = base.InitData(e);
            return data;
        }

        public static void ShowInfo(Window owner, string title, string message)
        {
            var data = new NotificationWindowData
            {
                Title = title,
                Message = message,
                NotificationType = NotificationType.Information,
                ShowCancel = false
            };
            var window = new NotificationWindow { Owner = owner };
            window.ForceInitData(data);
            window.ShowDialog();
        }

        public static void ShowError(Window owner, string title, string message)
        {
            var data = new NotificationWindowData
            {
                Title = title,
                Message = message,
                NotificationType = NotificationType.Error,
                ShowCancel = false
            };
            var window = new NotificationWindow { Owner = owner };
            window.ForceInitData(data);
            window.ShowDialog();
        }

        public static void ShowWarning(Window owner, string title, string message)
        {
            var data = new NotificationWindowData
            {
                Title = title,
                Message = message,
                NotificationType = NotificationType.Warning,
                ShowCancel = false
            };
            var window = new NotificationWindow { Owner = owner };
            window.ForceInitData(data);
            window.ShowDialog();
        }

        public static void ShowSuccess(Window owner, string title, string message)
        {
            var data = new NotificationWindowData
            {
                Title = title,
                Message = message,
                NotificationType = NotificationType.Success,
                ShowCancel = false
            };
            var window = new NotificationWindow { Owner = owner };
            window.ForceInitData(data);
            window.ShowDialog();
        }

        public static void ShowDetails(Window owner, string title, string subtitle, List<string> details, NotificationType type = NotificationType.Information)
        {
            var data = new NotificationWindowData
            {
                Title = title,
                Subtitle = subtitle,
                Details = new System.Collections.ObjectModel.ObservableCollection<string>(details),
                NotificationType = type,
                ShowCancel = false
            };
            var window = new NotificationWindow { Owner = owner };
            window.ForceInitData(data);
            window.ShowDialog();
        }

        public static bool ShowConfirmation(Window owner, string title, string message)
        {
            var data = new NotificationWindowData
            {
                Title = title,
                Message = message,
                NotificationType = NotificationType.Question,
                ShowCancel = true
            };
            var window = new NotificationWindow { Owner = owner };
            window.ForceInitData(data);
            return window.ShowDialog() == true;
        }
    }
}
