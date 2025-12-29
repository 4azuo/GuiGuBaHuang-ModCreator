using ModCreator.Attributes;
using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public ObservableCollection<string> ImageFiles { get; set; } = [];
        public ObservableCollection<FileItem> ImageItems { get; set; } = [];
        public List<ImageExtension> ImageExtensions { get; set; } = ResourceHelper.ReadEmbeddedResource<List<ImageExtension>>("ModCreator.Resources.image-extensions.json");
        public List<AudioExtension> AudioExtensions { get; set; } = ResourceHelper.ReadEmbeddedResource<List<AudioExtension>>("ModCreator.Resources.audio-extensions.json");
        [NotifyMethod(nameof(OnCustomResourceItemSelected))]
        public FileItem SelectedCustomResourceItem { get; set; }
        // Image folder filter
        public ObservableCollection<GameResourceFolderItem> ImageFolders { get; set; } = [];
        [NotifyMethod(nameof(OnSelectedImageFoldersChanged))]
        public string SelectedImageFolders { get; set; } = string.Empty;
        public bool HasSelectedCustomResourceItem => SelectedCustomResourceItem != null;

        // Game Resources - Separate collections per type
        public ObservableCollection<GameResourceItem> Texture2DItems { get; set; } = [];
        public ObservableCollection<GameResourceItem> SpriteItems { get; set; } = [];
        // public ObservableCollection<GameResourceItem> TextAssetItems { get; set; } = [];
        public ObservableCollection<GameResourceItem> AudioClipItems { get; set; } = [];
        public ObservableCollection<GameResourceItem> OtherItems { get; set; } = [];
        
        // Folder filter collections
        public ObservableCollection<GameResourceFolderItem> Texture2DFolders { get; set; } = [];
        public ObservableCollection<GameResourceFolderItem> SpriteFolders { get; set; } = [];
        // public ObservableCollection<GameResourceFolderItem> TextAssetFolders { get; set; } = [];
        public ObservableCollection<GameResourceFolderItem> AudioClipFolders { get; set; } = [];
        public ObservableCollection<GameResourceFolderItem> OtherFolders { get; set; } = [];
        
        [NotifyMethod(nameof(OnSelectedFoldersChanged))]
        public string SelectedTexture2DFolders { get; set; } = string.Empty;
        [NotifyMethod(nameof(OnSelectedFoldersChanged))]
        public string SelectedSpriteFolders { get; set; } = string.Empty;
        [NotifyMethod(nameof(OnSelectedFoldersChanged))]
        // public string SelectedTextAssetFolders { get; set; } = string.Empty;
        // [NotifyMethod(nameof(OnSelectedFoldersChanged))]
        public string SelectedAudioClipFolders { get; set; } = string.Empty;
        [NotifyMethod(nameof(OnSelectedFoldersChanged))]
        public string SelectedOtherFolders { get; set; } = string.Empty;
        
        private List<GameResourceItem> _allTexture2DResources = [];
        private List<GameResourceItem> _allSpriteResources = [];
        // private List<GameResourceItem> _allTextAssetResources = [];
        private List<GameResourceItem> _allAudioClipResources = [];
        private List<GameResourceItem> _allOtherResources = [];
        private List<FileItem> _allImageItems = [];
        
        [NotifyMethod(nameof(OnGameResourceItemSelected))]
        public GameResourceItem SelectedGameResourceItem { get; set; }
        public bool HasSelectedGameResource => SelectedGameResourceItem != null && !SelectedGameResourceItem.IsFolder;
        [NotifyMethod(nameof(OnGameResourceSearchTextChanged))]
        public string GameResourceSearchText { get; set; } = string.Empty;
        
        // Preview properties for game resources
        public BitmapImage SelectedResourceImagePath { get; set; }
        public string SelectedResourceAudioPath { get; set; }
        public bool IsCustomResource { get; set; }
        public bool IsResourceImage { get; set; }
        public bool IsResourceAudio { get; set; }
        
        // Loading state for game resources
        public bool IsLoadingGameResources { get; set; }

        public void LoadCustomResourceFiles()
        {
            if (Project == null) return;
            ResetResourcePreviews();

            var imgDir = Path.Combine(Project.ProjectPath, "ModProject", "ModImg");
            if (Directory.Exists(imgDir))
            {
                ImageFiles.ReplaceWith(Directory.EnumerateFiles(imgDir, "*", SearchOption.AllDirectories)
                    .Where(f => ImageExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()) ||
                                AudioExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()))
                    .Select(f => Path.GetRelativePath(imgDir, f)));

                _allImageItems = BuildCustomResourceFileTree(imgDir, imgDir);
                
                // Extract and populate image folders
                var folders = ExtractCustomResourceFolders(_allImageItems);
                ImageFolders.ReplaceWith(folders);
                
                FilterCustomResources();
            }
        }

        private List<FileItem> BuildCustomResourceFileTree(string rootPath, string currentPath, FileItem parent = null)
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

                var children = BuildCustomResourceFileTree(rootPath, dir, folderItem);
                foreach (var child in children)
                    folderItem.Children.Add(child);

                items.Add(folderItem);
            }

            var imageFiles = Directory.GetFiles(currentPath)
                .Where(f => ImageExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()) ||
                            AudioExtensions.Any(ext => ext.Extension == Path.GetExtension(f).ToLower()))
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

            if (SelectedCustomResourceItem == null || SelectedCustomResourceItem.IsFolder)
                return;

            var filePath = Path.Combine(Project.ProjectPath, "ModProject", "ModImg", SelectedCustomResourceItem.RelativePath);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            var extension = Path.GetExtension(filePath).ToLower();

            // Check if it's an image
            if (ImageExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceImagePath = BitmapHelper.LoadFromFile(filePath);
                IsResourceImage = true;
            }
            // Check if it's an audio file
            else if (AudioExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceAudioPath = filePath;
                IsResourceAudio = true;
            }
        }

        public void OnGameResourceItemSelected(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            // Reset preview properties
            ResetResourcePreviews();

            // Trigger property change notification for HasSelectedGameResource
            if (SelectedGameResourceItem == null || SelectedGameResourceItem.IsFolder)
                return;
            
            // Get the file path from Asset property
            var filePath = SelectedGameResourceItem.Asset as string;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;
            
            var extension = Path.GetExtension(filePath).ToLower();

            // Check if it's an image
            if (ImageExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceImagePath = BitmapHelper.LoadFromFile(filePath);
                IsResourceImage = true;
            }
            // Check if it's an audio file
            else if (AudioExtensions.Any(ext => ext.Extension == extension))
            {
                SelectedResourceAudioPath = filePath;
                IsResourceAudio = true;
            }
        }

        public void OnGameResourceSearchTextChanged(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            FilterCustomResources();
            FilterGameResourcesAsync();
        }

        public void OnSelectedFoldersChanged(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            // Re-filter resources when folder selection changes
            FilterGameResourcesAsync();
        }

        public void OnSelectedImageFoldersChanged(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            // Re-filter image items when folder selection changes
            FilterCustomResources();
        }

        private void FilterCustomResources()
        {
            if (_allImageItems == null || _allImageItems.Count == 0)
            {
                ImageItems.Clear();
                return;
            }

            var filtered = _allImageItems;

            // Apply folder filter
            if (!string.IsNullOrWhiteSpace(SelectedImageFolders))
            {
                var folderList = SelectedImageFolders.Split(',').Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToList();
                if (folderList.Any())
                {
                    filtered = FilterImageItemsByFolders(filtered, folderList);
                }
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(GameResourceSearchText))
            {
                filtered = FilterFileItems(filtered, GameResourceSearchText);
            }

            ImageItems.ReplaceWith(filtered);
        }

        private List<FileItem> FilterFileItems(List<FileItem> items, string searchText)
        {
            var result = new List<FileItem>();
            var lowerSearch = searchText.ToLower();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var filteredChildren = FilterFileItems(item.Children.ToList(), searchText);
                    if (filteredChildren.Count > 0)
                    {
                        var folderCopy = new FileItem
                        {
                            Name = item.Name,
                            FullPath = item.FullPath,
                            RelativePath = item.RelativePath,
                            IsFolder = true,
                            Parent = item.Parent
                        };
                        foreach (var child in filteredChildren)
                            folderCopy.Children.Add(child);
                        result.Add(folderCopy);
                    }
                }
                else if (item.Name.ToLower().Contains(lowerSearch))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private List<GameResourceFolderItem> ExtractCustomResourceFolders(List<FileItem> items)
        {
            var folders = new HashSet<string>();
            ExtractImageFoldersRecursive(items, folders, string.Empty);
            
            return folders.OrderBy(f => f).Select(f => new GameResourceFolderItem
            {
                FolderPath = f,
                IsSelected = false
            }).ToList();
        }

        private void ExtractImageFoldersRecursive(List<FileItem> items, HashSet<string> folders, string currentPath)
        {
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var folderPath = string.IsNullOrEmpty(currentPath) ? item.Name : $"{currentPath}/{item.Name}";
                    folders.Add(folderPath);
                    ExtractImageFoldersRecursive(item.Children.ToList(), folders, folderPath);
                }
            }
        }

        private List<FileItem> FilterImageItemsByFolders(List<FileItem> items, List<string> folders)
        {
            var result = new List<FileItem>();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    // Check if this folder or any parent folder matches
                    var folderPath = item.RelativePath.Replace("\\", "/");
                    var matches = folders.Any(f => folderPath.StartsWith(f, StringComparison.OrdinalIgnoreCase) || f.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase));
                    
                    if (matches)
                    {
                        var filteredChildren = FilterImageItemsByFolders(item.Children.ToList(), folders);
                        if (filteredChildren.Count > 0)
                        {
                            var folderCopy = new FileItem
                            {
                                Name = item.Name,
                                FullPath = item.FullPath,
                                RelativePath = item.RelativePath,
                                IsFolder = true,
                                Parent = item.Parent
                            };
                            foreach (var child in filteredChildren)
                                folderCopy.Children.Add(child);
                            result.Add(folderCopy);
                        }
                    }
                }
                else
                {
                    // Check if file is in any of the selected folders
                    var filePath = item.RelativePath.Replace("\\", "/");
                    var matches = folders.Any(f => filePath.StartsWith(f, StringComparison.OrdinalIgnoreCase));
                    if (matches)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private async Task FilterGameResourcesAsync()
        {
            try
            {
                // Run filtering operations in parallel for better performance
                await Task.WhenAll(
                    UpdateFilteredItemsAsync(GameResourceType.Texture2D),
                    UpdateFilteredItemsAsync(GameResourceType.Sprite),
                    // UpdateFilteredItemsAsync(GameResourceType.TextAsset),
                    UpdateFilteredItemsAsync(GameResourceType.AudioClip),
                    UpdateFilteredItemsAsync(GameResourceType.Other)
                );
            }
            catch (Exception ex)
            {
                DebugHelper.Error($"Failed to filter game resources: {ex.Message}");
            }
        }

        private async Task UpdateFilteredItemsAsync(GameResourceType resourceType)
        {
            await Task.Run(() =>
            {
                var business = Businesses.ProjectEditorWindowTab3GameResourceBusiness.Instance;
                List<GameResourceItem> allResources;
                string selectedFolders;
                
                switch (resourceType)
                {
                    case GameResourceType.Texture2D:
                        allResources = _allTexture2DResources;
                        selectedFolders = SelectedTexture2DFolders;
                        break;
                    case GameResourceType.Sprite:
                        allResources = _allSpriteResources;
                        selectedFolders = SelectedSpriteFolders;
                        break;
                    // case GameResourceType.TextAsset:
                    //     allResources = _allTextAssetResources;
                    //     selectedFolders = SelectedTextAssetFolders;
                    //     break;
                    case GameResourceType.AudioClip:
                        allResources = _allAudioClipResources;
                        selectedFolders = SelectedAudioClipFolders;
                        break;
                    case GameResourceType.Other:
                        allResources = _allOtherResources;
                        selectedFolders = SelectedOtherFolders;
                        break;
                    default:
                        return;
                }

                var filtered = allResources;

                // Apply folder filter
                if (!string.IsNullOrWhiteSpace(selectedFolders))
                {
                    var folderList = selectedFolders.Split(',').Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToList();
                    if (folderList.Any())
                    {
                        filtered = FilterByFolders(filtered, folderList);
                    }
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(GameResourceSearchText))
                {
                    filtered = business.SearchResources(filtered, GameResourceSearchText);
                }

                // Update UI collection on UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (resourceType)
                    {
                        case GameResourceType.Texture2D:
                            Texture2DItems.ReplaceWith(filtered);
                            break;
                        case GameResourceType.Sprite:
                            SpriteItems.ReplaceWith(filtered);
                            break;
                        // case GameResourceType.TextAsset:
                        //     TextAssetItems.ReplaceWith(filtered);
                        //     break;
                        case GameResourceType.AudioClip:
                            AudioClipItems.ReplaceWith(filtered);
                            break;
                        case GameResourceType.Other:
                            OtherItems.ReplaceWith(filtered);
                            break;
                    }
                });
            });
        }

        private List<GameResourceFolderItem> ExtractFolders(List<GameResourceItem> items)
        {
            var folders = new HashSet<string>();
            ExtractFoldersRecursive(items, folders);
            
            return folders.OrderBy(f => f).Select(f => new GameResourceFolderItem
            {
                FolderPath = f,
                IsSelected = false
            }).ToList();
        }

        private void ExtractFoldersRecursive(List<GameResourceItem> items, HashSet<string> folders)
        {
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var folderPath = GetItemPath(item);
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        folders.Add(folderPath);
                    }
                    ExtractFoldersRecursive(item.Children.ToList(), folders);
                }
            }
        }

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

        private List<GameResourceItem> FilterByFolders(List<GameResourceItem> items, List<string> selectedFolders)
        {
            var result = new List<GameResourceItem>();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var itemPath = GetItemPath(item);
                    
                    // Check if this folder or any parent folder is in selected folders
                    var isInSelectedFolder = selectedFolders.Any(folder => 
                        itemPath.Equals(folder, StringComparison.OrdinalIgnoreCase) || 
                        itemPath.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase));

                    if (isInSelectedFolder)
                    {
                        // Include entire folder
                        result.Add(item);
                    }
                    else
                    {
                        // Check if any child folders are in selected folders
                        var filteredChildren = FilterByFolders(item.Children.ToList(), selectedFolders);
                        if (filteredChildren.Any())
                        {
                            var folderCopy = new GameResourceItem
                            {
                                Name = item.Name,
                                Type = item.Type,
                                IsFolder = true,
                                Asset = item.Asset,
                                Parent = item.Parent
                            };
                            foreach (var child in filteredChildren)
                            {
                                folderCopy.Children.Add(child);
                            }
                            result.Add(folderCopy);
                        }
                    }
                }
            }

            return result;
        }

        public async Task LoadGameResourcesAsync()
        {
            var business = Businesses.ProjectEditorWindowTab3GameResourceBusiness.Instance;
            
            // Load each resource type independently and update UI as soon as it completes
            var texture2DTask = LoadAndUpdateResourceTypeAsync(GameResourceType.Texture2D, business);
            var spriteTask = LoadAndUpdateResourceTypeAsync(GameResourceType.Sprite, business);
            // var textAssetTask = LoadAndUpdateResourceTypeAsync(GameResourceType.TextAsset, business);
            var audioClipTask = LoadAndUpdateResourceTypeAsync(GameResourceType.AudioClip, business);
            var otherTask = LoadAndUpdateResourceTypeAsync(GameResourceType.Other, business);
            
            // Wait for all tasks to complete
            await Task.WhenAll(texture2DTask, spriteTask, audioClipTask, otherTask);
        }

        private async Task LoadAndUpdateResourceTypeAsync(GameResourceType resourceType, Businesses.ProjectEditorWindowTab3GameResourceBusiness business)
        {
            try
            {
                var resources = await business.LoadGameResourcesAsync(resourceType, false);
                
                // Extract unique folder paths for filtering
                var folders = ExtractFolders(resources);
                
                // Update the corresponding collection immediately after loading
                switch (resourceType)
                {
                    case GameResourceType.Texture2D:
                        _allTexture2DResources = resources;
                        Texture2DFolders.ReplaceWith(folders);
                        await UpdateFilteredItemsAsync(resourceType);
                        break;
                    
                    case GameResourceType.Sprite:
                        _allSpriteResources = resources;
                        SpriteFolders.ReplaceWith(folders);
                        await UpdateFilteredItemsAsync(resourceType);
                        break;
                    
                    // case GameResourceType.TextAsset:
                    //     _allTextAssetResources = resources;
                    //     TextAssetFolders.ReplaceWith(folders);
                    //     await UpdateFilteredItemsAsync(resourceType);
                    //     break;
                    
                    case GameResourceType.AudioClip:
                        _allAudioClipResources = resources;
                        AudioClipFolders.ReplaceWith(folders);
                        await UpdateFilteredItemsAsync(resourceType);
                        break;
                    
                    case GameResourceType.Other:
                        _allOtherResources = resources;
                        OtherFolders.ReplaceWith(folders);
                        await UpdateFilteredItemsAsync(resourceType);
                        break;
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Error($"Failed to load {resourceType} resources: {ex.Message}");
            }
        }
    }
}