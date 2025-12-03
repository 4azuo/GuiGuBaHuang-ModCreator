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
        public string ReturnType { get; set; }
        public ObservableCollection<string> Categories { get; set; } = [];
        [NotifyMethod(nameof(OnCategoryChanged))]
        public string SelectedCategory { get; set; }
        [NotifyMethod(nameof(OnSearchTextChanged))]
        public string SearchText { get; set; } = string.Empty;
        public List<EventActionBase> AllItems { get; set; } = [];
        public ObservableCollection<EventActionBase> FilteredItems { get; set; } = [];
        public EventActionBase SelectedItem { get; set; }
        public bool HasSelectedItem => SelectedItem != null;
        public List<GlobalVariable> AllVariables { get; set; } = [];
        public List<GlobalVariable> FilteredVariables { get; set; } = [];
        public GlobalVariable SelectedVariable { get; set; }
        public bool HasSelectedVariable => SelectedVariable != null;
        public bool HasSelectedItemOrVariable => SelectedItem != null || SelectedVariable != null;
        public bool ShowVariablesSection { get; set; } = false;
        public ModEventSelectType SelectType { get; set; }

        public void Initialize(ModEventItemType itemType, string returnType, string selectItemName, List<GlobalVariable> vars = null, Dictionary<int, ModEventItemSelectValue> parameterValues = null)
        {
            Begin();
            {
                ItemType = itemType;
                WindowTitle = itemType switch
                {
                    ModEventItemType.Event => "Select Event",
                    ModEventItemType.Action => "Select Action",
                    _ => "Select Item"
                };

                LoadItems();
                LoadCategories();
                LoadVariables(vars);
                SelectedCategory = "All";
                UpdateFilteredItems();

                // Pre-select item if SelectedItemName is provided
                if (!string.IsNullOrEmpty(selectItemName))
                {
                    var b = FilteredVariables.FirstOrDefault(a => a.Name == selectItemName);
                    if (b != null)
                    {
                        SelectedVariable = b;
                    }

                    var a = FilteredItems.FirstOrDefault(a => a.Name == selectItemName);
                    if (a != null)
                    {
                        SelectedItem = a;
                        SelectedItem.ParameterValues = parameterValues;
                    }
                }
            }
            End();
            NotifyAll();
        }

        private void LoadItems()
        {
            AllItems.Clear();

            switch (ItemType)
            {
                case ModEventItemType.Event:
                    AllItems.AddRange(ModEventHelper.LoadModEventMethodsFromAssembly());
                    break;
                case ModEventItemType.Action:
                    // Load from modevent-actions.json
                    AllItems.AddRange(ResourceHelper.ReadEmbeddedResource<List<EventActionBase>>("ModCreator.Resources.modevent-actions.json"));
                    // Load from ModLib.Helper.* classes
                    AllItems.AddRange(ModEventHelper.LoadModActionMethodsFromAssembly());
                    break;
            }

            if (!string.IsNullOrEmpty(ReturnType))
            {
                if (ReturnType == "dynamic")
                {
                    AllItems = AllItems.Where(item => item.IsReturn).ToList();
                }
                else
                {
                    AllItems = AllItems.Where(item => item.Return == ReturnType).ToList();
                }
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

        private void LoadVariables(List<GlobalVariable> vars = null)
        {
            AllVariables.AddRange(vars);

            if (!string.IsNullOrEmpty(ReturnType))
            {
                if (ReturnType != "dynamic")
                {
                    AllVariables = AllVariables.Where(item => item.Type == ReturnType).ToList();
                }
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
            // For events/actions filtering
            FilteredItems.Clear();

            var query1 = AllItems.AsEnumerable();

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                query1 = query1.Where(item => item.Category == SelectedCategory);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                query1 = query1.Where(item =>
                    item.DisplayName?.ToLower().Contains(searchLower) == true ||
                    item.Description?.ToLower().Contains(searchLower) == true ||
                    item.Name?.ToLower().Contains(searchLower) == true);
            }

            foreach (var item in query1)
                FilteredItems.Add(item);

            // For variables filtering
            FilteredVariables.Clear();

            var query2 = AllVariables.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                query2 = query2.Where(item =>
                    item.Description?.ToLower().Contains(searchLower) == true ||
                    item.Name?.ToLower().Contains(searchLower) == true);
            }

            foreach (var item in query2)
                FilteredVariables.Add(item);
        }
    }
}