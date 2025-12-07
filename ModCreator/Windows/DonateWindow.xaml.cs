using ModCreator.WindowData;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ModCreator.Windows
{
    public partial class DonateWindow : CWindow<DonateWindowData>
    {
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void KofiLink_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://ko-fi.com/fouru") { UseShellExecute = true });
        }
    }
}
