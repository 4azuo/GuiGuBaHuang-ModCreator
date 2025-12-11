using System.Collections.Generic;
using System.Reflection;
using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Helpers;
using Newtonsoft.Json;

namespace ModCreator.Models
{
    [SetterAspect]
    public class EventActionBase : AutoNotifiableObject
    {
        [JsonIgnore]
        public EventActionBase Parent { get; set; }
        [JsonIgnore]
        public string ComputedDisplayName => DisplayNameHelper.BuildNestedDisplayName(this);
        public string Category { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = [];
        public Dictionary<int, ModEventItemSelectValue> ParameterValues { get; set; } = [];
        public string Return { get; set; }
        public bool IsHidden { get; set; } = false;
        public bool IsCanAddChild { get; set; } = false;
        public List<string> SubItems { get; set; } = [];
        public bool IsReturn => !string.IsNullOrEmpty(Return) && Return != "Void";
        public string DisplayText => string.IsNullOrEmpty(Category) ? DisplayName : $"{Category} - {DisplayName}";
        [NotifyMethod(nameof(OnChildrenChanged))]
        public List<EventActionBase> Children { get; set; } = [];

        public void OnChildrenChanged(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            UpdateChildrenParent(this, Children);
        }

        private void UpdateChildrenParent(EventActionBase parent, List<EventActionBase> children)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Parent = parent;
                    UpdateChildrenParent(child, child.Children);
                }
            }
        }
    }
}