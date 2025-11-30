using ModCreator.Attributes;
using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    public class ModEventItemSelectWindowData : CWindowData
    {
        public string WindowTitle { get; set; } = "Select Item";
        public ModEventItemType ItemType { get; set; }
        public ObservableCollection<string> Categories { get; set; } = [];
        [NotifyMethod(nameof(OnCategoryChanged))]
        public string SelectedCategory { get; set; }
        [NotifyMethod(nameof(OnSearchTextChanged))]
        public string SearchText { get; set; } = string.Empty;
        public List<ModEventItemDisplay> AllItems { get; set; } = [];
        public ObservableCollection<ModEventItemDisplay> FilteredItems { get; set; } = [];
        public ModEventItemDisplay SelectedItem { get; set; }
        public bool HasSelectedItem => SelectedItem != null;

        public void Initialize(ModEventItemType itemType)
        {
            ItemType = itemType;
            WindowTitle = itemType switch
            {
                ModEventItemType.Event => "Select Event",
                ModEventItemType.Condition => "Select Condition",
                ModEventItemType.Action => "Select Action",
                _ => "Select Item"
            };

            LoadItems();
            LoadCategories();
            SelectedCategory = "All";
            UpdateFilteredItems();
        }

        private void LoadItems()
        {
            AllItems.Clear();

            switch (ItemType)
            {
                case ModEventItemType.Event:
                    var events = ResourceHelper.ReadEmbeddedResource<List<Models.EventInfo>>("ModCreator.Resources.modevent-events.json");
                    foreach (var evt in events)
                    {
                        AllItems.Add(new ModEventItemDisplay
                        {
                            Category = evt.Category,
                            Name = evt.Name,
                            DisplayName = evt.DisplayName,
                            Description = evt.Description,
                            Code = evt.Signature
                        });
                    }
                    break;

                case ModEventItemType.Condition:
                    var conditions = ResourceHelper.ReadEmbeddedResource<List<ConditionInfo>>("ModCreator.Resources.modevent-conditions.json");
                    foreach (var cond in conditions)
                    {
                        AllItems.Add(new ModEventItemDisplay
                        {
                            Category = cond.Category,
                            Name = cond.Name,
                            DisplayName = cond.DisplayName,
                            Description = cond.Description,
                            Code = cond.Code
                        });
                    }
                    break;

                case ModEventItemType.Action:
                    var actions = ResourceHelper.ReadEmbeddedResource<List<ActionInfo>>("ModCreator.Resources.modevent-actions.json");
                    foreach (var act in actions)
                    {
                        AllItems.Add(new ModEventItemDisplay
                        {
                            Category = act.Category,
                            Name = act.Name,
                            DisplayName = act.DisplayName,
                            Description = act.Description,
                            Code = act.Code
                        });
                    }
                    break;
            }
        }

        private void LoadCategories()
        {
            Categories.Clear();
            Categories.Add("All");
            foreach (var cat in AllItems.Select(i => i.Category).Distinct().OrderBy(c => c))
            {
                if (!string.IsNullOrEmpty(cat))
                    Categories.Add(cat);
            }
        }

        public void OnCategoryChanged(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            UpdateFilteredItems();
        }

        public void OnSearchTextChanged(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            UpdateFilteredItems();
        }

        private void UpdateFilteredItems()
        {
            FilteredItems.Clear();

            var query = AllItems.AsEnumerable();

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                query = query.Where(item => item.Category == SelectedCategory);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                query = query.Where(item =>
                    item.DisplayName?.ToLower().Contains(searchLower) == true ||
                    item.Description?.ToLower().Contains(searchLower) == true ||
                    item.Name?.ToLower().Contains(searchLower) == true);
            }

            foreach (var item in query)
                FilteredItems.Add(item);

            if (FilteredItems.Count == 1)
                SelectedItem = FilteredItems[0];
        }
    }
}