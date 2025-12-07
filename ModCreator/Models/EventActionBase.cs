using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Helpers;

namespace ModCreator.Models
{
    public class EventActionBase : AutoNotifiableObject
    {
        private ObservableCollection<EventActionBase> _children = [];

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

        // Computed property for displaying with nested parameters
        [Newtonsoft.Json.JsonIgnore]
        public string ComputedDisplayName => DisplayNameHelper.BuildNestedDisplayName(this);

        [Newtonsoft.Json.JsonIgnore]
        public EventActionBase Parent { get; set; }

        public ObservableCollection<EventActionBase> Children
        {
            get => _children;
            set
            {
                if (_children != null)
                    _children.CollectionChanged -= OnChildrenCollectionChanged;

                _children = value;

                if (_children != null)
                {
                    _children.CollectionChanged += OnChildrenCollectionChanged;
                    foreach (var child in _children)
                        child.Parent = this;
                }
            }
        }

        public EventActionBase()
        {
            _children.CollectionChanged += OnChildrenCollectionChanged;
        }

        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventActionBase child in e.NewItems)
                    child.Parent = this;
            }

            if (e.OldItems != null)
            {
                foreach (EventActionBase child in e.OldItems)
                    child.Parent = null;
            }
        }

        public void RefreshDisplayName()
        {
            Notify(nameof(ComputedDisplayName));
        }
    }
}