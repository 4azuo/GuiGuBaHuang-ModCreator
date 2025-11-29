using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    public class HelperWindowData : CWindowData
    {
        public List<DocItem> DocItems { get; set; } = new List<DocItem>();

        [NotifyMethod(nameof(LoadDocContent))]
        public DocItem SelectedDoc { get; set; }

        public string DocContent { get; set; }

        public override void OnLoad()
        {
            base.OnLoad();
            LoadDocumentList();
        }

        public void LoadDocumentList()
        {
            var docsPath = Constants.DocsDir;

            if (Directory.Exists(docsPath))
            {
                DocItems = BuildDocTree(docsPath, docsPath);
                var firstFile = FindFirstFile(DocItems);
                if (firstFile != null)
                    SelectedDoc = firstFile;
            }
            else
            {
                DocContent = "Documentation folder not found.\n\nExpected location: " + docsPath;
            }
        }

        private List<DocItem> BuildDocTree(string rootPath, string currentPath)
        {
            var items = new List<DocItem>();

            var directories = Directory.GetDirectories(currentPath).OrderBy(d => d);
            foreach (var dir in directories)
            {
                var folderItem = new DocItem
                {
                    Title = Path.GetFileName(dir),
                    IsFolder = true,
                    FilePath = dir
                };

                var children = BuildDocTree(rootPath, dir);
                foreach (var child in children)
                    folderItem.Children.Add(child);

                items.Add(folderItem);
            }

            var mdFiles = Directory.GetFiles(currentPath, "*.md").OrderBy(f => f);
            foreach (var file in mdFiles)
            {
                items.Add(new DocItem
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    FilePath = file,
                    IsFolder = false
                });
            }

            return items;
        }

        private DocItem FindFirstFile(List<DocItem> items)
        {
            foreach (var item in items)
            {
                if (!item.IsFolder)
                    return item;

                if (item.Children.Count > 0)
                {
                    var found = FindFirstFile(item.Children.ToList());
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        public void LoadDocContent(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedDoc == null)
            {
                DocContent = string.Empty;
                return;
            }

            DocContent = File.ReadAllText(SelectedDoc.FilePath);
        }
    }
}