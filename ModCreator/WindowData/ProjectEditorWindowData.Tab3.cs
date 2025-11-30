using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public List<string> ImageFiles { get; set; } = [];
        public ObservableCollection<FileItem> ImageItems { get; set; } = [];
        public List<ImageExtension> ImageExtensions { get; set; } = ResourceHelper.ReadEmbeddedResource<List<ImageExtension>>("ModCreator.Resources.image-extensions.json");
        public string SelectedImageFile { get; set; }
        [NotifyMethod(nameof(OnImageItemSelected))]
        public FileItem SelectedImageItem { get; set; }
        public BitmapImage SelectedImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedImageFile) || Project == null)
                    return null;

                var filePath = Path.Combine(Project.ProjectPath, "ModProject", "ModImg", SelectedImageFile);
                return BitmapHelper.LoadFromFile(filePath);
            }
        }

        public bool HasSelectedImageFile => !string.IsNullOrEmpty(SelectedImageFile);
        public bool HasSelectedImageItem => SelectedImageItem != null;

        public void OnImageItemSelected(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (SelectedImageItem == null || SelectedImageItem.IsFolder)
            {
                SelectedImageFile = null;
                return;
            }

            SelectedImageFile = SelectedImageItem.RelativePath;
        }

        public void LoadImageFiles()
        {
            ImageFiles.Clear();
            ImageItems.Clear();
            if (Project == null) return;

            var imgDir = Path.Combine(Project.ProjectPath, "ModProject", "ModImg");
            if (Directory.Exists(imgDir))
            {
                if (ImageExtensions.Count == 0)
                    throw new InvalidOperationException("ImageExtensions not loaded. Cannot load image files.");

                ImageFiles = Directory.EnumerateFiles(imgDir, "*", SearchOption.AllDirectories)
                    .Where(f => ImageExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()))
                    .Select(f => Path.GetRelativePath(imgDir, f))
                    .ToList();

                var items = BuildImageFileTree(imgDir, imgDir);
                foreach (var item in items)
                    ImageItems.Add(item);

                if (!string.IsNullOrEmpty(SelectedImageFile))
                {
                    var fullPath = Path.Combine(imgDir, SelectedImageFile);
                    if (!File.Exists(fullPath))
                    {
                        SelectedImageFile = null;
                        SelectedImageItem = null;
                    }
                }
            }
            else
            {
                SelectedImageFile = null;
                SelectedImageItem = null;
            }
        }

        private List<FileItem> BuildImageFileTree(string rootPath, string currentPath, FileItem parent = null)
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

                var children = BuildImageFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                    folderItem.Children.Add(child);

                items.Add(folderItem);
            }

            var imageFiles = Directory.GetFiles(currentPath)
                .Where(f => ImageExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()))
                .OrderBy(f => f);

            foreach (var file in imageFiles)
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
    }
}