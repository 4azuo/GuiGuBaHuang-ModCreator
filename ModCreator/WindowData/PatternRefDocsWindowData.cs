using ModCreator.Attributes;
using ModCreator.Commons;

namespace ModCreator.WindowData
{
    [SetterAspect]
    public class PatternRefDocsWindowData : CWindowData
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileContent { get; set; }
    }
}
