using ModCreator.Attributes;
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
        public List<string> ConfFiles { get; set; } = new List<string>();
        public ObservableCollection<FileItem> ConfItems { get; set; } = new ObservableCollection<FileItem>();

        [NotifyMethod(nameof(LoadConfContent))]
        public string SelectedConfFile { get; set; }

        [NotifyMethod(nameof(OnConfItemSelected))]
        public FileItem SelectedConfItem { get; set; }

        public string SelectedConfContent { get; set; }
        public bool HasSelectedConfFile => !string.IsNullOrEmpty(SelectedConfFile);
        public bool HasSelectedConfItem => SelectedConfItem != null;

        public void LoadConfFiles()
        {
            ConfFiles.Clear();
            ConfItems.Clear();
            if (Project == null) return;

            var confDir = Path.Combine(Project.ProjectPath, "ModProject", "ModConf");
            if (Directory.Exists(confDir))
            {
                ConfFiles = Directory.GetFiles(confDir, "*.json", SearchOption.AllDirectories)
                    .Select(f => Path.GetRelativePath(confDir, f))
                    .ToList();

                var items = BuildFileTree(confDir, confDir);
                foreach (var item in items)
                    ConfItems.Add(item);

                if (!string.IsNullOrEmpty(SelectedConfFile))
                {
                    var fullPath = Path.Combine(confDir, SelectedConfFile);
                    if (!File.Exists(fullPath))
                    {
                        SelectedConfFile = null;
                        SelectedConfItem = null;
                    }
                }
            }
            else
            {
                SelectedConfFile = null;
                SelectedConfItem = null;
            }
        }

        private List<FileItem> BuildFileTree(string rootPath, string currentPath, FileItem parent = null)
        {
            var items = new List<FileItem>();

            var directories = Directory.GetDirectories(currentPath).OrderBy(d => d);
            foreach (var dir in directories)
            {
                var folderItem = new FileItem
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    RelativePath = Path.GetRelativePath(rootPath, dir),
                    IsFolder = true,
                    Parent = parent
                };

                var children = BuildFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                    folderItem.Children.Add(child);

                items.Add(folderItem);
            }

            var jsonFiles = Directory.GetFiles(currentPath, "*.json").OrderBy(f => f);
            foreach (var file in jsonFiles)
            {
                items.Add(new FileItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    RelativePath = Path.GetRelativePath(rootPath, file),
                    IsFolder = false,
                    Parent = parent
                });
            }

            return items;
        }

        public void OnConfItemSelected(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedConfItem == null || SelectedConfItem.IsFolder)
            {
                SelectedConfFile = null;
                return;
            }

            SelectedConfFile = SelectedConfItem.RelativePath;
        }

        public void LoadConfContent(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (string.IsNullOrEmpty(SelectedConfFile) || Project == null)
            {
                SelectedConfContent = string.Empty;
                return;
            }

            var filePath = Path.Combine(Project.ProjectPath, "ModProject", "ModConf", SelectedConfFile);
            SelectedConfContent = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
        }

        public void SaveConfContent()
        {
            if (string.IsNullOrEmpty(SelectedConfFile) || Project == null || string.IsNullOrEmpty(SelectedConfContent))
                return;

            var filePath = Path.Combine(Project.ProjectPath, "ModProject", "ModConf", SelectedConfFile);
            File.WriteAllText(filePath, SelectedConfContent);
        }
    }
}