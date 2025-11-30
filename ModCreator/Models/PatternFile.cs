using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class PatternFile : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public List<string> Elements { get; set; }

        public PatternFile()
        {
            Elements = new List<string>();
        }
    }
}
