namespace ModCreator.Models
{
    public class ParameterInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsOptional { get; set; } = false;
        public string DefaultValue { get; set; } = string.Empty;
    }
}
