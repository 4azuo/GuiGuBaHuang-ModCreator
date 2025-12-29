using ModCreator.Attributes;
using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ModCreator.WindowData
{
    [SetterAspect]
    public class PatternResSelectWindowData : CWindowData
    {
        [IgnoredProperty, JsonIgnore]
        public CWindow<ProjectEditorWindowData> Editor { get; private set; }

        // Custom Resources (ModImg)
        public List<FileItem> CustomResourceItems { get; set; } = [];
        [NotifyMethod(nameof(OnCustomResourceItemSelected))]
        public FileItem SelectedCustomResource { get; set; }
        public bool IsCustomResourceSelected { get; set; } = false;

        // Game Resources
        public GameResourceType ResourceType { get; set; }
        public string ResourceFolder { get; set; }
        public bool HasResourceFolder => !string.IsNullOrEmpty(ResourceFolder);
        public List<GameResourceItem> GameResourceItems { get; set; } = [];
        [NotifyMethod(nameof(OnGameResourceItemSelected))]
        public GameResourceItem SelectedGameResource { get; set; }

        // UI properties
        public string StatusMessage { get; set; }
        public bool HasSelectedResource => SelectedCustomResource != null || SelectedGameResource != null;

        // Preview properties for game resources
        public BitmapImage SelectedResourceImagePath { get; set; }
        public string SelectedResourceAudioPath { get; set; }
        public bool IsCustomResource { get; set; }
        public bool IsResourceImage { get; set; }
        public bool IsResourceAudio { get; set; }
        
        // Loading state for game resources
        public bool IsLoadingGameResources { get; set; }

        public override void OnLoad()
        {
            base.OnLoad();
            Editor = Application.Current.Windows.OfType<CWindow<ProjectEditorWindowData>>().FirstOrDefault();

            // Load custom resources from Tab3 ImageItems
            CustomResourceItems = FilterCustomResourcesByFolder(Editor.WindowData.ImageItems, ResourceFolder);

            // Load game resources
            LoadGameResourcesAsync();

            // Set status message
            StatusMessage = "Ready";
        }

        public void ResetResourcePreviews()
        {
            SelectedResourceImagePath = null;
            SelectedResourceAudioPath = null;
            IsResourceImage = false;
            IsResourceAudio = false;
        }

        public void OnCustomResourceItemSelected(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            // Reset preview properties
            ResetResourcePreviews();
            IsCustomResourceSelected = true;

            if (SelectedCustomResource == null || SelectedCustomResource.IsFolder)
                return;

            var filePath = Path.Combine(Editor.WindowData.Project.ProjectPath, "ModProject", "ModImg", SelectedCustomResource.RelativePath);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            var extension = Path.GetExtension(filePath).ToLower();

            // Check if it's an image
            if (Editor.WindowData.ImageExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceImagePath = BitmapHelper.LoadFromFile(filePath);
                IsResourceImage = true;
            }
            // Check if it's an audio file
            else if (Editor.WindowData.AudioExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceAudioPath = filePath;
                IsResourceAudio = true;
            }
        }

        /// <summary>
        /// Filter file items by folder path
        /// </summary>
        private List<FileItem> FilterCustomResourcesByFolder(IEnumerable<FileItem> items, string folder)
        {
            var result = new List<FileItem>();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var folderPath = item.RelativePath.Replace("\\", "/");
                    if (folderPath.Contains(folder, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(item);
                    }
                    else
                    {
                        var children = FilterCustomResourcesByFolder(item.Children, folder);
                        if (children.Count > 0)
                        {
                            var folderCopy = new FileItem
                            {
                                Name = item.Name,
                                FullPath = item.FullPath,
                                RelativePath = item.RelativePath,
                                IsFolder = true,
                                Parent = item.Parent
                            };
                            foreach (var child in children)
                                folderCopy.Children.Add(child);
                            result.Add(folderCopy);
                        }
                    }
                }
                else
                {
                    var filePath = item.RelativePath.Replace("\\", "/");
                    if (filePath.Contains(folder, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Load game resources asynchronously
        /// </summary>
        public async Task LoadGameResourcesAsync()
        {
            try
            {
                IsLoadingGameResources = true;

                while (Editor.WindowData.IsLoadingGameResources)
                    await Task.Delay(1000);

                // Load game resources from Tab3
                var itemSources = ResourceType switch
                {
                    GameResourceType.Texture2D => Editor.WindowData.Texture2DItems,
                    GameResourceType.Sprite => Editor.WindowData.SpriteItems,
                    //GameResourceType.TextAsset => Editor.WindowData.TextAssetItems,
                    GameResourceType.AudioClip => Editor.WindowData.AudioClipItems,
                    GameResourceType.Other => Editor.WindowData.OtherItems,
                    _ => []
                };
                GameResourceItems = FilterGameResourcesByFolder(itemSources, ResourceFolder);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading game resources: {ex.Message}";
                DebugHelper.Log($"Failed to load game resources: {ex}");
            }
            finally
            {
                IsLoadingGameResources = false;
            }
        }

        public void OnGameResourceItemSelected(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            // Reset preview properties
            ResetResourcePreviews();
            IsCustomResourceSelected = false;

            // Trigger property change notification for HasSelectedGameResource
            if (SelectedGameResource == null || SelectedGameResource.IsFolder)
                return;

            // Get the file path from Asset property
            var filePath = SelectedGameResource.Asset as string;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            var extension = Path.GetExtension(filePath).ToLower();

            // Check if it's an image
            if (Editor.WindowData.ImageExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceImagePath = BitmapHelper.LoadFromFile(filePath);
                IsResourceImage = true;
            }
            // Check if it's an audio file
            else if (Editor.WindowData.AudioExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceAudioPath = filePath;
                IsResourceAudio = true;
            }
        }

        /// <summary>
        /// Filter game resources by folder path
        /// Reference: ProjectEditorWindowData.Tab3.cs FilterByFolders
        /// </summary>
        private List<GameResourceItem> FilterGameResourcesByFolder(IEnumerable<GameResourceItem> resources, string folder)
        {
            var list = new List<GameResourceItem>();

            foreach (var resource in resources)
            {
                if (resource.IsFolder)
                {
                    var itemPath = GetItemPath(resource);

                    // Check if this folder or any parent folder matches the target folder
                    var isInTargetFolder = itemPath.Equals(folder, StringComparison.OrdinalIgnoreCase) ||
                                          itemPath.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase);

                    if (isInTargetFolder)
                    {
                        // Include entire folder
                        list.Add(resource);
                    }
                    else
                    {
                        // Check if any child folders are in target folder
                        var filteredChildren = FilterGameResourcesByFolder(resource.Children, folder);
                        if (filteredChildren.Any())
                        {
                            var folderCopy = new GameResourceItem
                            {
                                Name = resource.Name,
                                PathInGame = resource.PathInGame,
                                IsFolder = true,
                                Type = resource.Type,
                                Asset = resource.Asset,
                                Parent = resource.Parent
                            };
                            foreach (var child in filteredChildren)
                            {
                                folderCopy.Children.Add(child);
                            }
                            list.Add(folderCopy);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Get full path of a game resource item
        /// </summary>
        private string GetItemPath(GameResourceItem item)
        {
            var parts = new List<string>();
            var current = item;

            while (current != null)
            {
                parts.Insert(0, current.Name);
                current = current.Parent;
            }

            return string.Join("/", parts);
        }
    }
}
