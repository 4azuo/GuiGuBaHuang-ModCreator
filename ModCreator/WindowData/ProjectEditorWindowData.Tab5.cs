using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
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
        public List<EventActionBase> AvailableEvents { get; set; } = ModEventHelper.LoadModEventMethodsFromAssembly();

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

                if (SelectedModEvent.Conditions.Count == 0 || SelectedModEvent.Conditions[0].Name != "Root")
                {
                    SelectedModEvent.Conditions.Insert(0, new EventActionBase
                    {
                        Name = "Root",
                        DisplayName = "Root",
                        Code = "Root"
                    });
                }

                if (SelectedModEvent.Actions.Count == 0 || SelectedModEvent.Actions[0].Name != "Root")
                {
                    SelectedModEvent.Actions.Insert(0, new EventActionBase
                    {
                        Name = "Root",
                        DisplayName = "Root",
                        Code = "Root"
                    });
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
            
            // Update the content in the corresponding FileItem
            if (SelectedEventItem != null && !SelectedEventItem.IsFolder && SelectedModEvent != null)
            {
                SelectedEventItem.Content = EventSourceContent;
            }
        }

        public void SaveModEvent()
        {
            if (SelectedModEvent == null || string.IsNullOrEmpty(SelectedModEvent.FilePath))
                return;

            var content = IsGuiMode ? GenerateModEventCode(SelectedModEvent) : EventSourceContent;
            File.WriteAllText(SelectedModEvent.FilePath, content);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFile", Path.GetFileName(SelectedModEvent.FilePath));
        }

        public void SaveModEvents()
        {
            if (Project == null)
                return;

            // Update current item's content
            if (SelectedEventItem != null && !SelectedEventItem.IsFolder && SelectedModEvent != null)
            {
                var content = IsGuiMode ? GenerateModEventCode(SelectedModEvent) : EventSourceContent;
                SelectedEventItem.Content = content;
            }

            // Save all files in EventItems
            int savedCount = 0;
            SaveAllEventItems(EventItems, ref savedCount);
            
            if (savedCount > 0)
                StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFiles", savedCount);
        }

        private void SaveAllEventItems(ObservableCollection<FileItem> items, ref int savedCount)
        {
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    // Recursively save children
                    SaveAllEventItems(item.Children, ref savedCount);
                }
                else
                {
                    // Find the ModEventItem for this file
                    var modEvent = Project?.ModEvents?.FirstOrDefault(e => e.FilePath == item.FullPath);
                    if (modEvent != null)
                    {
                        var content = string.Empty;
                        
                        // If this is the selected item, use current GUI/code mode
                        if (item == SelectedEventItem)
                        {
                            content = IsGuiMode ? GenerateModEventCode(modEvent) : EventSourceContent;
                        }
                        else
                        {
                            // For other items, check if they have stored content or generate from ModEventItem
                            if (!string.IsNullOrEmpty(item.Content))
                            {
                                content = item.Content;
                            }
                            else if (!modEvent.IsCodeModeOnly)
                            {
                                content = GenerateModEventCode(modEvent);
                            }
                            else if (File.Exists(item.FullPath))
                            {
                                // Code mode only, keep existing file content
                                content = File.ReadAllText(item.FullPath);
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(content))
                        {
                            File.WriteAllText(item.FullPath, content);
                            savedCount++;
                        }
                    }
                }
            }
        }

        public string GenerateModEventCode(ModEventItem modEvent)
        {
            return string.Empty; // Todo
        }
    }
}
