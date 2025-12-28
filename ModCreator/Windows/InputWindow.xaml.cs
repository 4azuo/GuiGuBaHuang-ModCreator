using ModCreator.WindowData;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class InputWindow : CWindow<InputWindowData>
    {
        public override void OnLoad()
        {
            base.OnLoad();
            var textBox = this.FindName("txtInput") as System.Windows.Controls.TextBox;
            textBox?.Focus();
        }

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
    }
}