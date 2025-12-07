using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ModCreator.Helpers
{
    public static class ResourceHelper
    {
        public static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static T ReadEmbeddedResource<T>(string resourceName)
        {
            var json = ReadEmbeddedResource(resourceName);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static BitmapImage ReadEmbeddedImage(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
