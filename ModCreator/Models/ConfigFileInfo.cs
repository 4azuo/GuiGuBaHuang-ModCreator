using ModCreator.Attributes;
using ModCreator.Commons;

namespace ModCreator.Models
{
    [SetterAspect]
    public class ConfigFileInfo : AutoNotifiableObject
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
    }
}
