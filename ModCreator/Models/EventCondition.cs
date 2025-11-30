using ModCreator.Commons;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    public class EventCondition : AutoNotifiableObject
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public ObservableCollection<EventCondition> Children { get; set; } = [];
        public string DisplayText => string.IsNullOrEmpty(Category) ? DisplayName : $"{Category} - {DisplayName}";
    }
}
