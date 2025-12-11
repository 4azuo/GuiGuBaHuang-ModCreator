using ModCreator.Attributes;

namespace ModCreator.WindowData
{
    [SetterAspect]
    public class InputWindowData : CWindowData
    {
        public string WindowTitle { get; set; } = "Input";
        public string Label { get; set; } = "Value:";
        public string InputValue { get; set; } = string.Empty;
    }
}