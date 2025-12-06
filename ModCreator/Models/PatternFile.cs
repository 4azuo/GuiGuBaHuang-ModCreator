using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class PatternFile : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public List<string> Elements { get; set; } = new List<string>();
        public int FrozenColumns { get; set; } = 2;
    }
}
