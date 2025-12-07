using ModCreator.Helpers;
using System.Windows.Media.Imaging;

namespace ModCreator.WindowData
{
    public class DonateWindowData : CWindowData
    {
        public BitmapImage DonateImage { get; set; } = BitmapHelper.Base64ToBitmapImage(Constants.DONATE_QR_BASE64);
    }
}
