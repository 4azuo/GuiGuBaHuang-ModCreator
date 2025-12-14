using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public List<FileItem> EventItems { get; set; } = [];
        [NotifyMethod(nameof(OnEventItemSelected))]
        public FileItem SelectedEventItem { get; set; }
        [NotifyMethod(nameof(LoadModEventContent))]
        public ModEventItem SelectedModEvent
        {
            get => (ModEventItem)SelectedEventItem?.ObjectContent;
            set
            {
                if (SelectedEventItem != null)
                    SelectedEventItem.ObjectContent = value;
            }
        }
        public string EventSourceContent
        {
            get => SelectedEventItem?.Content;
            set
            {
                if (SelectedEventItem != null)
                    SelectedEventItem.Content = value;
            }
        }
        public bool HasSelectedEventFile => SelectedModEvent != null;
        public bool IsCodeModeOnly => SelectedModEvent?.IsCodeModeOnly == true;
        public bool IsGuiModeEnabled => SelectedModEvent != null && !IsCodeModeOnly;
        public List<string> CacheTypes => ModEventHelper.LoadCacheTypes();
        public List<string> WorkOnTypes => ModEventHelper.LoadWorkOnTypes();
        public List<EventActionBase> AvailableEvents { get; set; } = ModEventHelper.LoadModEventMethodsFromAssembly();

        public bool CanUndo => SelectedModEvent?.CanUndo ?? false && !IsCodeModeOnly;
        public bool CanRedo => SelectedModEvent?.CanRedo ?? false && !IsCodeModeOnly;
        public bool CanSwitchEventToCodeMode => !IsCodeModeOnly;

        public void UndoModEvent()
        {
            if (SelectedEventItem == null)
                return;

            if (!IsCodeModeOnly)
            {
                SelectedModEvent.Undo();
            }

            StatusMessage = MessageHelper.Get("Messages.Success.Undo");
        }

        public void RedoModEvent()
        {
            if (SelectedEventItem == null)
                return;

            if (!IsCodeModeOnly)
            {
                SelectedModEvent.Redo();
            }

            StatusMessage = MessageHelper.Get("Messages.Success.Redo");
        }

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
                    Parent = parent,
                    Content = File.ReadAllText(file),
                    ObjectContent = Project?.ModEvents?.FirstOrDefault(e => e.FilePath == file)
                });
            }

            return items;
        }

        public void OnEventItemSelected(object obj, PropertyInfo prop, object before = null, object after = null)
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

                if (SelectedModEvent.Conditions.Count == 0 || SelectedModEvent.Conditions[0].Name != Constants.EventActionRootElement.Name)
                {
                    SelectedModEvent.Conditions.Insert(0, Constants.EventActionRootElement);
                }

                if (SelectedModEvent.Actions.Count == 0 || SelectedModEvent.Actions[0].Name != Constants.EventActionRootElement.Name)
                {
                    SelectedModEvent.Actions.Insert(0, Constants.EventActionRootElement);
                }
            }
            else
                SelectedModEvent = null;
        }

        public void LoadModEventContent(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            EventSourceContent = SelectedModEvent != null && File.Exists(SelectedModEvent.FilePath)
                ? File.ReadAllText(SelectedModEvent.FilePath)
                : string.Empty;
        }

        public void SaveModEvent(FileItem file, bool showStatusMsg = true)
        {
            if (file == null || string.IsNullOrEmpty(file.FullPath))
                return;

            var modEventItem = file.GetObjectContentAs<ModEventItem>();
            if (modEventItem == null)
                return;

            var content = file.Content;
            File.WriteAllText(file.FullPath, content);
            if (showStatusMsg)
                StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFile", Path.GetFileName(SelectedModEvent.FilePath));
        }

        public void SaveModEvents()
        {
            if (Project == null)
                return;

            // Save all files in EventItems
            int savedCount = 0;
            foreach (var item in EventItems)
            {
                SaveModEvent(item);
                savedCount++;
            }

            if (savedCount > 0)
                StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedModEventFiles", savedCount);
        }
    }
}
