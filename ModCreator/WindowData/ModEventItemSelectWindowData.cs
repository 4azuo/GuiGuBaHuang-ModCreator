using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace ModCreator.WindowData
{
    public class ModEventItemSelectWindowData : CWindowData
    {
        public string WindowTitle { get; set; } = "Select Item";
        public ModEventItemType ItemType { get; set; }
        public string ReturnType { get; set; }
        public bool HasReturn => !string.IsNullOrEmpty(ReturnType) && ReturnType != "Void";
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
        public string OptionalValue { get; set; }
        public bool HasOptionalValue => !string.IsNullOrWhiteSpace(OptionalValue);
        public bool HasAnyChange => SelectedItem != null || SelectedVariable != null || HasOptionalValue;
        public bool HasNoChange => SelectedItem == null && SelectedVariable == null;
        public bool HasSelect => SelectedItem != null;
        public bool HasSelectVariable => SelectedVariable != null;
        public bool HasParameters => SelectedItem?.Parameters != null && SelectedItem.Parameters.Count > 0;
        public bool ShowOptionalValueSection { get; set; } = false;
        public ModEventSelectType SelectType { get; set; }

        public void ClearSelection()
        {
            Begin();
            {
                SelectedItem = null;
                SelectedVariable = null;
            }
            End();
            NotifyAll();
        }

        public void Initialize(ModEventItemType itemType, string returnType, string selectItemName, Dictionary<int, ModEventItemSelectValue> parameterValues = null)
        {
            Begin();
            {
                ItemType = itemType;
                ReturnType = returnType;
                WindowTitle = itemType switch
                {
                    ModEventItemType.Event => "Select Event",
                    ModEventItemType.Action => "Select Action",
                    _ => "Select Item"
                };

                LoadItems();
                LoadCategories();
                LoadVariables();
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
                switch (ReturnType)
                {
                    case "Object":
                        AllItems = AllItems.Where(item => item.IsReturn).ToList();
                        break;
                    default:
                        if (ValidatedModel.VarTypes.Any(x => x.Type == ReturnType))
                            AllItems = AllItems.Where(item => item.Return == ReturnType).ToList();
                        else
                            AllItems = AllItems.Where(item => item.IsReturn).ToList();
                        break;

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

        private void LoadVariables()
        {
            var editor = Application.Current.Windows.OfType<CWindow<ProjectEditorWindowData>>().FirstOrDefault();
            AllVariables.AddRange(editor.WindowData.GlobalVariables);

            if (!string.IsNullOrEmpty(ReturnType))
            {
                if (ReturnType != "Object")
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