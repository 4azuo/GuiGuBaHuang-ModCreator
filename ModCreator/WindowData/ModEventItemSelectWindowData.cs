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
        public List<ModEventItem> AllNonEvents { get; set; } = [];
        public List<ModEventItem> FilteredNonEvents { get; set; } = [];
        public ModEventItem SelectedNonEvent { get; set; }
        public bool IsNonEventMode => ReturnType == "NonEvent";
        public bool IsNotNonEventMode => ReturnType != "NonEvent";
        public string OptionalValue { get; set; }
        public bool HasOptionalValue => !string.IsNullOrWhiteSpace(OptionalValue);
        public bool HasAnyChange => SelectedItem != null || SelectedVariable != null || SelectedNonEvent != null || HasOptionalValue;
        public bool HasNoChange => SelectedItem == null && SelectedVariable == null && SelectedNonEvent == null;
        public bool HasSelect => SelectedItem != null;
        public bool HasSelectVariable => SelectedVariable != null;
        public bool HasParameters => SelectedItem?.Parameters != null && SelectedItem.Parameters.Count > 0;
        public bool ShowOptionalValueSection { get; set; } = false;
        public ModEventSelectType SelectType { get; set; }
        public ObservableCollection<ModConfTreeNode> ModConfTree { get; set; } = [];
        public ModConfTreeNode SelectedModConfNode { get; set; }

        public void ClearSelection()
        {
            Begin();
            {
                SelectedItem = null;
                SelectedVariable = null;
                SelectedNonEvent = null;
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
                LoadNonEvents();
                LoadModConf();
                SelectedCategory = "All";
                UpdateFilteredItems();

                if (IsNonEventMode)
                {
                    // Pre-select NonEvent if SelectedItemName is provided
                    if (!string.IsNullOrEmpty(selectItemName))
                    {
                        var c = FilteredNonEvents.FirstOrDefault(a => a.FileName == selectItemName);
                        if (c != null)
                        {
                            SelectedNonEvent = c;
                        }
                    }
                }
                // Pre-select item if SelectedItemName is provided
                else if (!string.IsNullOrEmpty(selectItemName))
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

        private void LoadNonEvents()
        {
            AllNonEvents.Clear();

            var editor = Application.Current.Windows.OfType<CWindow<ProjectEditorWindowData>>().FirstOrDefault();
            if (editor != null)
            {
                var nonEventFiles = GetNonEventFiles(editor.WindowData.Project.ModEvents);
                AllNonEvents.AddRange(nonEventFiles);
            }
        }

        private List<ModEventItem> GetNonEventFiles(List<ModEventItem> items)
        {
            return items.Where(x => x.EventMode == EventMode.NonEvent).ToList();
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

            // For NonEvents filtering
            FilteredNonEvents.Clear();

            var query3 = AllNonEvents.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                query3 = query3.Where(item =>
                    item.FileName?.ToLower().Contains(searchLower) == true);
            }

            foreach (var item in query3)
                FilteredNonEvents.Add(item);
        }

        private void LoadModConf()
        {
            ModConfTree.Clear();

            var editor = Application.Current.Windows.OfType<CWindow<ProjectEditorWindowData>>().FirstOrDefault();
            if (editor?.WindowData?.ConfItems == null) return;

            foreach (var fileItem in editor.WindowData.ConfItems)
            {
                var node = BuildModConfTree(fileItem);
                if (node != null && node.Children.Count > 0)
                    ModConfTree.Add(node);
            }
        }

        private ModConfTreeNode BuildModConfTree(FileItem fileItem)
        {
            var node = new ModConfTreeNode
            {
                DisplayName = fileItem.Name,
                Type = fileItem.IsFolder ? ModConfNodeType.Folder : ModConfNodeType.File,
                FilePath = fileItem.FullPath
            };

            if (fileItem.IsFolder)
            {
                foreach (var child in fileItem.Children)
                {
                    var childNode = BuildModConfTree(child);
                    if (childNode != null && (childNode.Type == ModConfNodeType.Folder && childNode.Children.Count > 0 || childNode.Type != ModConfNodeType.Folder))
                        node.Children.Add(childNode);
                }
            }
            else
            {
                // Parse JSON file to extract ModEventParam elements
                var confNodes = ParseModConfFile(fileItem.FullPath);
                foreach (var confNode in confNodes)
                    node.Children.Add(confNode);
            }

            return node;
        }

        private Dictionary<string, ModConfElement> LoadPatternInfoForFile(string fileName)
        {
            var result = new Dictionary<string, ModConfElement>();
            var allElements = ModConfHelper.LoadElements();

            // Filter elements by matching FileName
            foreach (var element in allElements.Values)
            {
                if (element.FileName == fileName)
                {
                    var key = element.Name.Contains(".") 
                        ? element.Name.Substring(element.Name.IndexOf('.') + 1) 
                        : element.Name;
                    result[key] = element;
                }
            }

            return result;
        }

        private List<ModConfTreeNode> ParseModConfFile(string filePath)
        {
            var nodes = new List<ModConfTreeNode>();

            var jsonContent = System.IO.File.ReadAllText(filePath);
            var jsonArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);

            if (jsonArray == null) return nodes;

            var fileName = System.IO.Path.GetFileName(filePath);
            var patternInfo = LoadPatternInfoForFile(fileName);

            foreach (var jsonObj in jsonArray)
            {
                foreach (var kvp in jsonObj)
                {
                    // Check if this field should be included (ModEventParam = true)
                    if (patternInfo != null && patternInfo.ContainsKey(kvp.Key))
                    {
                        var elementInfo = patternInfo[kvp.Key];
                        if (elementInfo.ModEventParam)
                        {
                            var label = elementInfo.Label ?? kvp.Key;
                            var labelNode = new ModConfTreeNode
                            {
                                DisplayName = elementInfo.Label ?? kvp.Key,
                                Type = ModConfNodeType.Label,
                                FieldName = label
                            };

                            var value = kvp.Value?.ToString() ?? "(empty)";
                            var valueNode = new ModConfTreeNode
                            {
                                DisplayName = value,
                                Type = ModConfNodeType.Value,
                                Value = value,
                                FieldName = kvp.Key,
                            };

                            labelNode.Children.Add(valueNode);
                            nodes.Add(labelNode);
                        }
                    }
                }
            }

            return nodes;
        }
    }
}