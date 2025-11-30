using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class ModConfElement : AutoNotifiableObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string VarType { get; set; }
        public bool Enable { get; set; } = true;
        public bool Required { get; set; } = false;
        public List<string> Options { get; set; } = new List<string>();
        public string Value { get; set; }
        public string Separator { get; set; }
        public List<string> SubProperties { get; set; } = new List<string>();
    }
}
