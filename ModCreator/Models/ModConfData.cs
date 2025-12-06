using System.Collections.Generic;

namespace ModCreator.Models
{
    public class ModConfData
    {
        public Dictionary<string, ModConfElement> Elements { get; set; }
        public List<RegularPattern> Patterns { get; set; }
    }
}
