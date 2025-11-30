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
        public bool Enable { get; set; }
        public bool Required { get; set; }
        public List<string> Options { get; set; }
        public string Value { get; set; }

        public ModConfElement()
        {
            Options = new List<string>();
            Enable = true;
            Required = false;
        }
    }
}
