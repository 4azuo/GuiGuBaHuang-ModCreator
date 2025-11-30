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

            if (Project.ModEvents == null)
                Project.ModEvents = [];
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
            {
                var existingEvent = Project?.ModEvents?.FirstOrDefault(e => e.FilePath == SelectedEventItem.FullPath);
                if (existingEvent != null)
                {
                    SelectedModEvent = existingEvent;
                }
                else
                {
                    SelectedModEvent = new ModEventItem { FilePath = SelectedEventItem.FullPath };
                    Project?.ModEvents?.Add(SelectedModEvent);
                }
            }
            else
                SelectedModEvent = null;
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
