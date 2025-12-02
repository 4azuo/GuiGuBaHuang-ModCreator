using System.Collections.Generic;

namespace ModCreator.Models
{
    public class EventActionBase
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
        public string Return { get; set; }
        public bool HasBody { get; set; } = false;
        public bool IsHidden { get; set; } = false;
        public bool IsCanAddChild { get; set; } = true;
        public List<string> SubItems { get; set; } = new List<string>();
        public bool IsReturn => !string.IsNullOrEmpty(Return) && Return != "Void";
        public string DisplayText => string.IsNullOrEmpty(Category) ? DisplayName : $"{Category} - {DisplayName}";
    }
}
