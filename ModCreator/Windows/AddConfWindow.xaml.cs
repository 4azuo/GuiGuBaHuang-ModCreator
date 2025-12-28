using ModCreator.WindowData;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class AddConfWindow : CWindow<AddConfWindowData>
    {
        public override void OnLoad()
        {
            base.OnLoad();
            WindowData.LoadConfigurations();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedConfig != null)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}