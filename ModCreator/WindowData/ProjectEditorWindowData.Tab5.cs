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
using EventInfo = ModCreator.Models.EventInfo;

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
        public List<string> CacheTypes { get; set; } = [];
        public List<string> WorkOnTypes { get; set; } = [];
        public List<EventInfo> AvailableEvents { get; set; } = ResourceHelper.ReadEmbeddedResource<List<EventInfo>>("ModCreator.Resources.modevent-events.json");

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
                foreach (var part in conditionParts)
                {
                    modEvent.Conditions.Add(new EventCondition
                    {
                        Code = part.Trim().TrimEnd(';')
                    });
                }
            }

            var actionMatches = ActionRegex.Matches(source);
            foreach (Match match in actionMatches)
            {
                var actionCode = match.Groups[1].Value.Trim();
                if (!actionCode.StartsWith("if") && !actionCode.StartsWith("return") && !actionCode.Contains("CheckCondition"))
                {
                    modEvent.Actions.Add(new EventAction
                    {
                        Code = actionCode
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
            StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFile", Path.GetFileName(SelectedModEvent.FilePath));
        }

        public string GenerateModEventCode(ModEventItem modEvent)
        {
            return string.Empty; // Todo
        }
    }
}
