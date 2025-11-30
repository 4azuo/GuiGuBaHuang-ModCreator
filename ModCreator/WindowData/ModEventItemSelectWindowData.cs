using ModCreator.Attributes;
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
        public string ItemType { get; set; }
        public ObservableCollection<string> Categories { get; set; } = [];

        [NotifyMethod(nameof(OnCategoryChanged))]
        public string SelectedCategory { get; set; }

        [NotifyMethod(nameof(OnSearchTextChanged))]
        public string SearchText { get; set; } = string.Empty;

        public List<object> AllItems { get; set; } = [];
        public ObservableCollection<object> FilteredItems { get; set; } = [];
        public object SelectedItem { get; set; }
        public bool HasSelectedItem => SelectedItem != null;

        public void InitializeWithEvents(List<EventCategory> eventCategories)
        {
            ItemType = "Event";
            WindowTitle = "Select Event";

            Categories.Clear();
            Categories.Add("All");
            foreach (var cat in eventCategories.Select(c => c.Category).Distinct().OrderBy(c => c))
                Categories.Add(cat);

            AllItems.Clear();
            foreach (var category in eventCategories)
            {
                foreach (var evt in category.Events)
                {
                    AllItems.Add(new EventInfoDisplay
                    {
                        Category = category.Category,
                        Name = evt.Name,
                        DisplayName = evt.DisplayName,
                        Description = evt.Description,
                        Code = evt.Signature,
                        EventInfo = evt
                    });
                }
            }

            SelectedCategory = "All";
            UpdateFilteredItems();
        }

        public void InitializeWithConditions(List<ConditionInfo> conditions)
        {
            ItemType = "Condition";
            WindowTitle = "Select Condition";

            Categories.Clear();
            Categories.Add("All");
            foreach (var cat in conditions.Select(c => c.Category).Distinct().OrderBy(c => c))
                Categories.Add(cat);

            AllItems = conditions.Cast<object>().ToList();

            SelectedCategory = "All";
            UpdateFilteredItems();
        }

        public void InitializeWithActions(List<ActionInfo> actions)
        {
            ItemType = "Action";
            WindowTitle = "Select Action";

            Categories.Clear();
            Categories.Add("All");
            foreach (var cat in actions.Select(a => a.Category).Distinct().OrderBy(c => c))
                Categories.Add(cat);

            AllItems = actions.Cast<object>().ToList();

            SelectedCategory = "All";
            UpdateFilteredItems();
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
                query = query.Where(item =>
                {
                    if (item is EventInfoDisplay evt) return evt.Category == SelectedCategory;
                    if (item is ConditionInfo cond) return cond.Category == SelectedCategory;
                    if (item is ActionInfo act) return act.Category == SelectedCategory;
                    return false;
                });
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                query = query.Where(item =>
                {
                    if (item is EventInfoDisplay evt)
                        return evt.DisplayName?.ToLower().Contains(searchLower) == true ||
                               evt.Description?.ToLower().Contains(searchLower) == true ||
                               evt.Name?.ToLower().Contains(searchLower) == true;
                    if (item is ConditionInfo cond)
                        return cond.DisplayName?.ToLower().Contains(searchLower) == true ||
                               cond.Description?.ToLower().Contains(searchLower) == true ||
                               cond.Name?.ToLower().Contains(searchLower) == true;
                    if (item is ActionInfo act)
                        return act.DisplayName?.ToLower().Contains(searchLower) == true ||
                               act.Description?.ToLower().Contains(searchLower) == true ||
                               act.Name?.ToLower().Contains(searchLower) == true;
                    return false;
                });
            }

            foreach (var item in query)
                FilteredItems.Add(item);

            if (FilteredItems.Count == 1)
                SelectedItem = FilteredItems[0];
        }
    }
}