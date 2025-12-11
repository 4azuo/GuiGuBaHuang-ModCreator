using ModCreator.Attributes;
using ModCreator.Commons;

namespace ModCreator.Models
{
    [SetterAspect]
    public class ModConfValue : ValidatedModel
    {
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public bool IsSelected { get; set; }
    }
}
