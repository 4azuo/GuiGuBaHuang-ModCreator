using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class RegularPattern : AutoNotifiableObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PatternFile> Files { get; set; } = new List<PatternFile>();
    }
}
