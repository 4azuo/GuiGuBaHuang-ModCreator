using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace ModCreator.WindowData
{
    public class PatternSelectorWindowData : CWindowData
    {
        private List<RegularPattern> _allPatterns;
        private Dictionary<string, ModConfElement> _allElements;
        private Dictionary<string, List<ModConfValue>> _allValues;
        private string _projectPath;

        [NotifyMethod(nameof(FilterPatterns))]
        public string SearchText { get; set; }

        [NotifyMethod(nameof(LoadExistingFiles))]
        public string Prefix { get; set; }

        public ObservableCollection<RegularPattern> FilteredPatterns { get; set; }

        private RegularPattern _selectedPattern;
        public RegularPattern SelectedPattern
        {
            get => _selectedPattern;
            set
            {
                _selectedPattern = value;
                UpdateDisplayFiles();
            }
        }

        public ObservableCollection<PatternFileDisplay> DisplayFiles { get; set; }

        public bool HasSelectedPattern => SelectedPattern != null;

        public bool HasExistingFiles
        {
            get
            {
                if (string.IsNullOrEmpty(_projectPath) || SelectedPattern == null)
                    return false;

                var confPath = System.IO.Path.Combine(_projectPath, "ModProject", "ModConf");
                if (!System.IO.Directory.Exists(confPath))
                    return false;

                foreach (var file in SelectedPattern.Files)
                {
                    var fileName = GetPrefixedFileName(file.FileName);
                    var filePath = System.IO.Path.Combine(confPath, fileName);

                    if (System.IO.File.Exists(filePath))
                        return true;
                }

                return false;
            }
        }

        public PatternSelectorWindowData()
        {
            _allPatterns = new List<RegularPattern>();
            _allElements = new Dictionary<string, ModConfElement>();
            _allValues = new Dictionary<string, List<ModConfValue>>();
            FilteredPatterns = new ObservableCollection<RegularPattern>();
            DisplayFiles = new ObservableCollection<PatternFileDisplay>();
            LoadPatterns();
        }

        private string GetPrefixedFileName(string fileName)
        {
            if (string.IsNullOrEmpty(Prefix))
                return fileName;

            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var ext = System.IO.Path.GetExtension(fileName);
            return $"{Prefix}_{fileNameWithoutExt}{ext}";
        }

        private void UpdateDisplayFiles()
        {
            DisplayFiles.Clear();
            if (SelectedPattern == null)
                return;

            foreach (var file in SelectedPattern.Files)
            {
                var displayFile = new PatternFileDisplay { FileName = file.FileName };
                foreach (var elementName in file.Elements)
                {
                    var element = ResolveElement(elementName);
                    if (element != null && element.Enable)
                        displayFile.Elements.Add(element);
                }
                displayFile.AddRow();
                DisplayFiles.Add(displayFile);
            }

            LoadExistingFiles(this, null, null, null);
        }

        public void SetProjectPath(string projectPath)
        {
            _projectPath = projectPath;
            Notify(this, nameof(HasExistingFiles));
        }

        public void LoadExistingFiles(object obj, System.Reflection.PropertyInfo prop, object oldValue, object newValue)
        {
            if (string.IsNullOrEmpty(_projectPath) || SelectedPattern == null)
            {
                Notify(this, nameof(HasExistingFiles));
                return;
            }

            var confPath = System.IO.Path.Combine(_projectPath, "ModProject", "ModConf");
            if (!System.IO.Directory.Exists(confPath))
                return;

            foreach (var file in DisplayFiles)
            {
                var fileName = GetPrefixedFileName(file.FileName);
                var filePath = System.IO.Path.Combine(confPath, fileName);

                if (!System.IO.File.Exists(filePath))
                    continue;

                var jsonContent = FileHelper.ReadTextFile(filePath);
                if (string.IsNullOrEmpty(jsonContent))
                    continue;

                if (jsonContent.TrimStart().StartsWith("["))
                {
                    var jsonArray = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);
                    if (jsonArray != null && jsonArray.Count > 0)
                    {
                        file.Rows.Clear();
                        foreach (var jsonObject in jsonArray)
                        {
                            var row = new Dictionary<string, string>();
                            foreach (var element in file.Elements)
                            {
                                if (jsonObject.ContainsKey(element.Name))
                                    row[element.Name] = jsonObject[element.Name]?.ToString() ?? string.Empty;
                                else
                                    row[element.Name] = element.Value ?? string.Empty;
                            }
                            file.Rows.Add(new RowDisplay(row, file.Elements));
                        }
                    }
                }
                else
                {
                    var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
                    if (jsonObject != null && file.Rows.Count > 0)
                    {
                        var firstRow = file.Rows[0];
                        foreach (var element in file.Elements)
                        {
                            if (jsonObject.ContainsKey(element.Name))
                            {
                                firstRow.RowData[element.Name] = jsonObject[element.Name]?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }
            }

            Notify(this, nameof(HasExistingFiles));
        }

        public void LoadPatterns()
        {
            _allPatterns.Clear();
            _allElements.Clear();
            _allValues.Clear();

            _allPatterns = ResourceHelper.ReadEmbeddedResource<List<RegularPattern>>("ModCreator.Resources.modconf-patterns.json");
            FilterPatterns(this, null, null, null);

            var elements = ResourceHelper.ReadEmbeddedResource<List<ModConfElement>>("ModCreator.Resources.modconfs.json");
            foreach (var element in elements)
                _allElements[element.Name] = element;

            var valueGroups = ResourceHelper.ReadEmbeddedResource<List<ModConfValueGroup>>("ModCreator.Resources.modconf-values.json");
            foreach (var group in valueGroups)
                _allValues[group.Name] = group.Values;
        }

        public PatternElement ResolveElement(string elementName)
        {
            if (!_allElements.TryGetValue(elementName, out var element))
                return null;

            var patternElement = new PatternElement
            {
                Name = elementName.Contains(".") ? elementName.Split('.')[1] : elementName,
                Type = element.Type,
                Label = element.Label,
                Description = element.Description,
                VarType = element.VarType,
                Enable = element.Enable,
                Required = element.Required,
                Value = element.Value
            };

            if (element.Options != null)
            {
                foreach (var optionRef in element.Options)
                {
                    if (_allValues.TryGetValue(optionRef, out var values))
                    {
                        patternElement.Options.AddRange(values);
                    }
                }
            }

            return patternElement;
        }

        public void FilterPatterns(object obj, System.Reflection.PropertyInfo prop, object oldValue, object newValue)
        {
            FilteredPatterns.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allPatterns
                : _allPatterns.Where(p => p.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) 
                    || p.Description.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase));

            foreach (var pattern in filtered.OrderBy(p => p.Name))
                FilteredPatterns.Add(pattern);
        }
    }
}
