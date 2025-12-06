using ModCreator.Commons;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ModCreator.WindowData
{
    public enum NotificationType
    {
        Information,
        Warning,
        Error,
        Question,
        Success
    }

    public class NotificationWindowData : CWindowData
    {
        public string Title { get; set; } = "Notification";
        public string Subtitle { get; set; }
        public string Message { get; set; }
        public ObservableCollection<string> Details { get; set; } = new ObservableCollection<string>();
        public NotificationType NotificationType { get; set; } = NotificationType.Information;
        public bool ShowCancel { get; set; } = false;

        public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
        public bool IsSimpleMessage => !string.IsNullOrWhiteSpace(Message) && Details.Count == 0;
        public bool HasDetails => Details.Count > 0;

        public Brush HeaderBackground
        {
            get
            {
                return NotificationType switch
                {
                    NotificationType.Information => new SolidColorBrush(Color.FromRgb(46, 80, 144)), // #FF2E5090
                    NotificationType.Warning => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // #FFFF9800
                    NotificationType.Error => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // #FFF44336
                    NotificationType.Question => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // #FF2196F3
                    NotificationType.Success => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // #FF4CAF50
                    _ => new SolidColorBrush(Color.FromRgb(46, 80, 144))
                };
            }
        }
    }
}
