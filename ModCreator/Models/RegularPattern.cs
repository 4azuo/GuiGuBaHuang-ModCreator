using ModCreator.Attributes;
using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class RegularPattern
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PatternFile> Files { get; set; } = new List<PatternFile>();
    }
}
