using ModCreator.Attributes;
using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class PatternFile
    {
        public string FileName { get; set; }
        public List<string> Elements { get; set; } = [];
        public int FrozenColumns { get; set; }
    }
}
