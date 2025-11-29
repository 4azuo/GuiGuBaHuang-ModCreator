using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        private static readonly Regex CacheAttributeRegex = new(@"\[Cache\([""'](.+?)[""'],\s*CacheType\s*=\s*(.+?),\s*WorkOn\s*=\s*(.+?),\s*OrderIndex\s*=\s*(\d+)\)\]", RegexOptions.Compiled);
        private static readonly Regex EventMethodRegex = new(@"public override void (On\w+)\([^)]*\)", RegexOptions.Compiled);
        private static readonly Regex ConditionRegex = new(@"return (.+?);", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ActionRegex = new(@"(?:^|\n)\s+(.+?;)(?=\s*(?:\n|$))", RegexOptions.Multiline | RegexOptions.Compiled);

        public ObservableCollection<FileItem> EventItems { get; set; } = [];

        [NotifyMethod(nameof(OnEventItemSelected))]
        public FileItem SelectedEventItem { get; set; }

        [NotifyMethod(nameof(LoadModEventContent))]
        public ModEventItem SelectedModEvent { get; set; }

        public string EventSourceContent { get; set; }
        public bool HasSelectedEventFile => SelectedModEvent != null;
        public bool IsGuiMode { get; set; } = true;
        public List<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
        public List<ConditionInfo> AvailableConditions { get; set; } = new List<ConditionInfo>();
        public List<ActionInfo> AvailableActions { get; set; } = new List<ActionInfo>();
        public List<string> EventCategoryList { get; set; } = new List<string>();
        public List<string> ConditionCategoryList { get; set; } = new List<string>();
        public List<string> ActionCategoryList { get; set; } = new List<string>();
        public List<string> CacheTypes { get; set; } = [];
        public List<string> WorkOnTypes { get; set; } = [];
        public List<string> EventModeOptions { get; set; } = new List<string> { "ModEvent", "NonEvent" };

        public void LoadModEventFiles()
        {
            if (Project == null) return;

            var modPath = Path.Combine(Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Mod");

            if (!Directory.Exists(modPath))
                Directory.CreateDirectory(modPath);

            EventItems.Clear();
            var items = BuildEventFileTree(modPath, modPath);
            foreach (var item in items)
                EventItems.Add(item);
        }

        private List<FileItem> BuildEventFileTree(string rootPath, string currentPath, FileItem parent = null)
        {
            var items = new List<FileItem>();

            var directories = Directory.GetDirectories(currentPath).OrderBy(d => d);
            foreach (var dir in directories)
            {
                var folderItem = new FileItem
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    IsFolder = true,
                    Parent = parent
                };

                var children = BuildEventFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                    folderItem.Children.Add(child);

                items.Add(folderItem);
            }

            var files = Directory.GetFiles(currentPath, "*.cs").OrderBy(f => f);
            foreach (var file in files)
            {
                items.Add(new FileItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsFolder = false,
                    Parent = parent
                });
            }

            return items;
        }

        public void OnEventItemSelected(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedEventItem != null && !SelectedEventItem.IsFolder)
                LoadModEventFromFile(SelectedEventItem.FullPath);
            else
                SelectedModEvent = null;
        }

        private void LoadModEventFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var content = File.ReadAllText(filePath);
            SelectedModEvent = ParseModEventFromSource(content, filePath);
        }

        private ModEventItem ParseModEventFromSource(string source, string filePath)
        {
            var modEvent = new ModEventItem { FilePath = filePath };

            var cacheMatch = CacheAttributeRegex.Match(source);
            if (cacheMatch.Success)
            {
                modEvent.CacheType = cacheMatch.Groups[2].Value.Trim();
                modEvent.WorkOn = cacheMatch.Groups[3].Value.Trim();
                modEvent.OrderIndex = int.Parse(cacheMatch.Groups[4].Value);
            }

            var eventMatch = EventMethodRegex.Match(source);
            if (eventMatch.Success)
                modEvent.SelectedEvent = eventMatch.Groups[1].Value;

            var conditionMatch = ConditionRegex.Match(source);
            if (conditionMatch.Success)
            {
                var conditionCode = conditionMatch.Groups[1].Value.Trim();
                modEvent.ConditionLogic = conditionCode.Contains("&&") ? "AND" : "OR";

                var separator = modEvent.ConditionLogic == "AND" ? "&&" : "||";
                var conditionParts = conditionCode.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                int order = 0;
                foreach (var part in conditionParts)
                {
                    modEvent.Conditions.Add(new EventCondition
                    {
                        Code = part.Trim().TrimEnd(';'),
                        Order = order++
                    });
                }
            }

            var actionMatches = ActionRegex.Matches(source);
            int actionOrder = 0;
            foreach (Match match in actionMatches)
            {
                var actionCode = match.Groups[1].Value.Trim();
                if (!actionCode.StartsWith("if") && !actionCode.StartsWith("return") && !actionCode.Contains("CheckCondition"))
                {
                    modEvent.Actions.Add(new EventAction
                    {
                        Code = actionCode,
                        Order = actionOrder++
                    });
                }
            }

            return modEvent;
        }

        public void LoadModEventContent(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            EventSourceContent = SelectedModEvent != null && File.Exists(SelectedModEvent.FilePath)
                ? File.ReadAllText(SelectedModEvent.FilePath)
                : string.Empty;
        }

        public void SaveModEvent()
        {
            if (SelectedModEvent == null || string.IsNullOrEmpty(SelectedModEvent.FilePath))
                return;

            var content = IsGuiMode ? GenerateModEventCode(SelectedModEvent) : EventSourceContent;
            File.WriteAllText(SelectedModEvent.FilePath, content);
        }

        public string GenerateModEventCode(ModEventItem modEvent)
        {
            var templatePath = Path.Combine(Project.ProjectPath, "EventTemplate.tmp");
            var contentTemplatePath = Path.Combine(Project.ProjectPath, "EventTemplateContent.tmp");

            if (!File.Exists(templatePath) || !File.Exists(contentTemplatePath))
                return string.Empty;

            var template = File.ReadAllText(templatePath);
            var contentTemplate = File.ReadAllText(contentTemplatePath);

            var eventInfo = EventCategories.SelectMany(c => c.Events)
                .FirstOrDefault(e => e.Name == modEvent.SelectedEvent);

            if (eventInfo == null)
                return string.Empty;

            var conditionCode = GenerateConditionCode(modEvent);
            var actionCode = GenerateActionCode(modEvent);

            var eventContent = contentTemplate
                .Replace("#EVENTMETHOD#", eventInfo.Signature)
                .Replace("#CONDITION#", conditionCode)
                .Replace("#ACTION#", actionCode);

            var code = template
                .Replace("#CLASSNAME#", modEvent.FileName)
                .Replace("#CACHETYPE#", modEvent.CacheType)
                .Replace("#WORKON#", modEvent.WorkOn)
                .Replace("#ORDERINDEX#", modEvent.OrderIndex.ToString())
                .Replace("#EVENTCONTENT#", eventContent);

            return code;
        }

        private string GenerateConditionCode(ModEventItem modEvent)
        {
            if (modEvent.Conditions == null || modEvent.Conditions.Count == 0)
                return "true";

            var separator = modEvent.ConditionLogic == "AND" ? " && " : " || ";
            var conditions = modEvent.Conditions.OrderBy(c => c.Order)
                .Select(c => $"({c.Code})");

            return string.Join(separator, conditions);
        }

        private string GenerateActionCode(ModEventItem modEvent)
        {
            if (modEvent.Actions == null || modEvent.Actions.Count == 0)
                return "// No actions";

            var actions = modEvent.Actions.OrderBy(a => a.Order)
                .Select(a => $"        {a.Code}");

            return string.Join("\n", actions);
        }

        public void LoadModEventResources()
        {
            var resourcePrefix = "ModCreator.Resources.";

            EventCategories = ResourceHelper.ReadEmbeddedResource<List<EventCategory>>(resourcePrefix + "modevent-events.json");
            AvailableConditions = ResourceHelper.ReadEmbeddedResource<List<ConditionInfo>>(resourcePrefix + "modevent-conditions.json");
            AvailableActions = ResourceHelper.ReadEmbeddedResource<List<ActionInfo>>(resourcePrefix + "modevent-actions.json");
            CacheTypes = ResourceHelper.ReadEmbeddedResource<List<string>>(resourcePrefix + "modevent-cachetype.json");
            WorkOnTypes = ResourceHelper.ReadEmbeddedResource<List<string>>(resourcePrefix + "modevent-workon.json");

            var cats = ResourceHelper.ReadEmbeddedResource<Dictionary<string, List<string>>>(resourcePrefix + "modevent-cats.json");
            EventCategoryList = cats["EventCategories"];
            ConditionCategoryList = cats["ConditionCategories"];
            ActionCategoryList = cats["ActionCategories"];
        }
    }
}
