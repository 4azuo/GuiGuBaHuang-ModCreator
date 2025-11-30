using ModCreator.Attributes;
using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class PatternElement : ValidatedModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string VarType { get; set; }
        public bool Enable { get; set; } = true;
        public bool Required { get; set; } = false;
        public List<ModConfValue> Options { get; set; } = new List<ModConfValue>();
        public string Value { get; set; }
    }
}
