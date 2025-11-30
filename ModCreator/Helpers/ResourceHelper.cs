using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace ModCreator.Helpers
{
    public static class ResourceHelper
    {
        private static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static T ReadEmbeddedResource<T>(string resourceName)
        {
            var json = ReadEmbeddedResource(resourceName);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
