using ModCreator.Enums;
using ModCreator.Helpers;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModCreator.Businesses
{
    /// <summary>
    /// Business logic for managing game resources from BaseGameResources directory
    /// </summary>
    public class ProjectEditorWindowTab3GameResourceBusiness
    {
        private static ProjectEditorWindowTab3GameResourceBusiness _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static ProjectEditorWindowTab3GameResourceBusiness Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new ProjectEditorWindowTab3GameResourceBusiness();
                }
            }
        }

        private ProjectEditorWindowTab3GameResourceBusiness() { }

        /// <summary>
        /// Initialize resource scanning from BaseGameResources directory
        /// </summary>
        /// <returns>Tuple of (success, errors list)</returns>
        public (bool success, List<string> errors) InitializeResources(string gameFolderPath = null)
        {
            var errors = new List<string>();
            
            try
            {
                var basePath = Constants.BaseGameResourcesDir;
                
                if (string.IsNullOrEmpty(basePath))
                {
                    errors.Add("BaseGameResourcesDir is not configured.");
                    return (false, errors);
                }

                if (!Directory.Exists(basePath))
                {
                    errors.Add($"BaseGameResources folder not found: {basePath}");
                    return (false, errors);
                }

                return (true, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to initialize resource scanner: {ex.Message}");
                return (false, errors);
            }
        }

        /// <summary>
        /// Initialize resource scanner asynchronously
        /// </summary>
        public (bool success, List<string> errors) InitializeResourcesAsync(string gameFolderPath = null)
        {
            return InitializeResources(gameFolderPath);
        }

        /// <summary>
        /// Load game resources tree from BaseGameResources directory asynchronously (cached)
        /// Structure: BaseGameResources/[category]/[type]/file.ext
        /// Example: BaseGameResources 1.2.111/sounds/bg/AudioClip/beijinglong.wav
        /// Result: "beijinglong.wav" in folder "sounds/bg" with type AudioClip
        /// </summary>
        /// <param name="resourceType">Type of resources to load</param>
        /// <param name="forceReload">Force reload from disk, ignoring cache</param>
        public async Task<List<GameResourceItem>> LoadGameResourcesAsync(GameResourceType resourceType, bool forceReload = false)
        {
            try
            {
                var basePath = Constants.BaseGameResourcesDir;
                
                if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                {
                    return new List<GameResourceItem>();
                }

                // Load resources by type using helper (with caching)
                return await BaseGameResourceHelper.LoadResourcesByTypeAsync(basePath, resourceType, forceReload);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game resources: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<GameResourceItem>();
            }
        }

        /// <summary>
        /// Load all game resources asynchronously (all types in parallel)
        /// </summary>
        /// <param name="forceReload">Force reload from disk, ignoring cache</param>
        public async Task<List<GameResourceItem>> LoadAllGameResourcesAsync(bool forceReload = false)
        {
            try
            {
                var basePath = Constants.BaseGameResourcesDir;
                
                if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                {
                    return new List<GameResourceItem>();
                }

                // Load all resource types in parallel
                var resourceTypes = new[]
                {
                    GameResourceType.AudioClip,
                    GameResourceType.Sprite,
                    GameResourceType.Texture2D,
                    GameResourceType.TextAsset,
                    GameResourceType.Other
                };

                var loadTasks = resourceTypes.Select(type => 
                    BaseGameResourceHelper.LoadResourcesByTypeAsync(basePath, type, forceReload)
                ).ToArray();

                var results = await Task.WhenAll(loadTasks);

                // Merge all results into a single tree
                return MergeResourceTrees(results);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game resources: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<GameResourceItem>();
            }
        }

        /// <summary>
        /// Merge multiple resource trees into a single tree
        /// </summary>
        private List<GameResourceItem> MergeResourceTrees(List<GameResourceItem>[] trees)
        {
            var mergedFolders = new Dictionary<string, GameResourceItem>();

            foreach (var tree in trees)
            {
                foreach (var item in tree)
                {
                    MergeItemIntoTree(item, mergedFolders);
                }
            }

            // Convert flat dictionary to hierarchical tree
            return BuildHierarchicalTree(mergedFolders);
        }

        /// <summary>
        /// Recursively merge an item and its children into the tree
        /// </summary>
        private void MergeItemIntoTree(GameResourceItem item, Dictionary<string, GameResourceItem> folders)
        {
            if (item.IsFolder)
            {
                var path = GetItemPath(item);
                
                if (!folders.ContainsKey(path))
                {
                    folders[path] = new GameResourceItem
                    {
                        Name = item.Name,
                        Type = GameResourceType.Folder,
                        IsFolder = true,
                        Asset = item.Asset
                    };
                }

                // Merge children
                foreach (var child in item.Children.ToList())
                {
                    MergeItemIntoTree(child, folders);
                }
            }
            else
            {
                // Add file to its parent folder
                if (item.Parent != null)
                {
                    var parentPath = GetItemPath(item.Parent);
                    
                    if (!folders.ContainsKey(parentPath))
                    {
                        folders[parentPath] = new GameResourceItem
                        {
                            Name = item.Parent.Name,
                            Type = GameResourceType.Folder,
                            IsFolder = true,
                            Asset = item.Parent.Asset
                        };
                    }

                    // Check if file already exists
                    if (!folders[parentPath].Children.Any(c => c.Name == item.Name && c.Asset == item.Asset))
                    {
                        var fileItem = new GameResourceItem
                        {
                            Name = item.Name,
                            Type = item.Type,
                            IsFolder = false,
                            Asset = item.Asset,
                            Parent = folders[parentPath]
                        };
                        folders[parentPath].Children.Add(fileItem);
                    }
                }
            }
        }

        /// <summary>
        /// Get full path of an item
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

        /// <summary>
        /// Build hierarchical tree from flat folder dictionary
        /// </summary>
        private List<GameResourceItem> BuildHierarchicalTree(Dictionary<string, GameResourceItem> flatFolders)
        {
            var rootItems = new List<GameResourceItem>();
            var folderCache = new Dictionary<string, GameResourceItem>();

            // Sort by path to ensure parents are processed before children
            var sortedFolders = flatFolders.OrderBy(kvp => kvp.Key).ToList();

            foreach (var kvp in sortedFolders)
            {
                var path = kvp.Key;
                var item = kvp.Value;

                // Split path into parts
                var parts = path.Split('/');

                if (parts.Length == 1)
                {
                    // Top-level folder
                    if (!folderCache.ContainsKey(path))
                    {
                        folderCache[path] = item;
                        rootItems.Add(item);
                    }
                }
                else
                {
                    // Find or create parent folder
                    var parentPath = string.Join("/", parts.Take(parts.Length - 1));
                    
                    if (!folderCache.ContainsKey(parentPath))
                    {
                        // Create parent folder chain
                        var parent = CreateFolderChain(parentPath, folderCache, rootItems);
                        folderCache[parentPath] = parent;
                    }

                    var parentFolder = folderCache[parentPath];
                    
                    // Update item name to show only the last part
                    item.Name = parts[parts.Length - 1];
                    item.Parent = parentFolder;
                    
                    if (!parentFolder.Children.Any(c => c.Name == item.Name))
                    {
                        parentFolder.Children.Add(item);
                    }
                    
                    folderCache[path] = item;
                }
            }

            return rootItems;
        }

        /// <summary>
        /// Create folder chain for nested paths
        /// </summary>
        private GameResourceItem CreateFolderChain(string path, Dictionary<string, GameResourceItem> cache, List<GameResourceItem> rootItems)
        {
            if (cache.ContainsKey(path))
                return cache[path];

            var parts = path.Split('/');
            
            if (parts.Length == 1)
            {
                // Create root folder
                var folder = new GameResourceItem
                {
                    Name = parts[0],
                    Type = GameResourceType.Folder,
                    IsFolder = true
                };
                cache[path] = folder;
                rootItems.Add(folder);
                return folder;
            }
            else
            {
                // Create parent chain first
                var parentPath = string.Join("/", parts.Take(parts.Length - 1));
                var parent = CreateFolderChain(parentPath, cache, rootItems);

                // Create this folder
                var folder = new GameResourceItem
                {
                    Name = parts[parts.Length - 1],
                    Type = GameResourceType.Folder,
                    IsFolder = true,
                    Parent = parent
                };
                parent.Children.Add(folder);
                cache[path] = folder;
                return folder;
            }
        }

        /// <summary>
        /// Search resources by name
        /// </summary>
        public List<GameResourceItem> SearchResources(List<GameResourceItem> items, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return items;
            }

            var results = new List<GameResourceItem>();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var childResults = SearchResources(item.Children.ToList(), searchText);
                    if (childResults.Any())
                    {
                        var folderCopy = new GameResourceItem
                        {
                            Name = item.Name,
                            Type = item.Type,
                            IsFolder = true,
                            Asset = item.Asset
                        };

                        foreach (var child in childResults)
                        {
                            folderCopy.Children.Add(child);
                        }

                        results.Add(folderCopy);
                    }
                }
                else if (item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(item);
                }
            }

            return results;
        }

        /// <summary>
        /// Filter resources by type
        /// </summary>
        public List<GameResourceItem> FilterByType(List<GameResourceItem> items, GameResourceType type)
        {
            if (type == GameResourceType.Folder)
            {
                return items;
            }

            var results = new List<GameResourceItem>();

            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    var childResults = FilterByType(item.Children.ToList(), type);
                    if (childResults.Any())
                    {
                        var folderCopy = new GameResourceItem
                        {
                            Name = item.Name,
                            Type = item.Type,
                            IsFolder = true,
                            Asset = item.Asset
                        };

                        foreach (var child in childResults)
                        {
                            folderCopy.Children.Add(child);
                        }

                        results.Add(folderCopy);
                    }
                }
                else if (item.Type == type)
                {
                    results.Add(item);
                }
            }

            return results;
        }

        /// <summary>
        /// Get resource count by type
        /// </summary>
        public Dictionary<GameResourceType, int> GetResourceCount(List<GameResourceItem> items)
        {
            return BaseGameResourceHelper.GetResourceCount(items);
        }

        /// <summary>
        /// Reset cached resources (force reload on next access)
        /// </summary>
        public void Reset()
        {
            BaseGameResourceHelper.ClearCache();
        }
    }
}
