using ModCreator.Enums;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper for loading and parsing game resources from BaseGameResources directory
    /// </summary>
    public static class BaseGameResourceHelper
    {
        private static Dictionary<GameResourceType, List<GameResourceItem>> _cachedResourcesByType;
        private static string _cachedBasePath;
        private static readonly object _lock = new object();
        private static readonly Dictionary<GameResourceType, Task<List<GameResourceItem>>> _loadingTasks = new Dictionary<GameResourceType, Task<List<GameResourceItem>>>();

        /// <summary>
        /// Clear cached resources (force reload on next access)
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _cachedResourcesByType = null;
                _cachedBasePath = null;
                _loadingTasks.Clear();
            }
        }

        /// <summary>
        /// Load resources asynchronously by type
        /// </summary>
        /// <param name="basePath">Base directory path</param>
        /// <param name="resourceType">Resource type to load</param>
        /// <param name="forceReload">Force reload from disk, ignoring cache</param>
        public static async Task<List<GameResourceItem>> LoadResourcesByTypeAsync(string basePath, GameResourceType resourceType, bool forceReload = false)
        {
            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
            {
                return new List<GameResourceItem>();
            }

            lock (_lock)
            {
                // Return cached resources if available and not forcing reload
                if (!forceReload && _cachedResourcesByType != null && _cachedBasePath == basePath)
                {
                    if (_cachedResourcesByType.ContainsKey(resourceType))
                    {
                        return _cachedResourcesByType[resourceType];
                    }
                }

                // Check if already loading this type
                if (_loadingTasks.ContainsKey(resourceType))
                {
                    return _loadingTasks[resourceType].Result;
                }
            }

            // Create loading task
            var loadTask = Task.Run(() => LoadResourcesByType(basePath, resourceType, forceReload));
            
            lock (_lock)
            {
                _loadingTasks[resourceType] = loadTask;
            }

            var result = await loadTask;

            lock (_lock)
            {
                _loadingTasks.Remove(resourceType);
            }

            return result;
        }

        /// <summary>
        /// Load resources synchronously by type (used internally by async version)
        /// </summary>
        private static List<GameResourceItem> LoadResourcesByType(string basePath, GameResourceType resourceType, bool forceReload = false)
        {
            lock (_lock)
            {
                // Initialize cache if needed
                if (_cachedResourcesByType == null || _cachedBasePath != basePath || forceReload)
                {
                    _cachedResourcesByType = new Dictionary<GameResourceType, List<GameResourceItem>>();
                    _cachedBasePath = basePath;
                }

                // Return cached if available
                if (_cachedResourcesByType.ContainsKey(resourceType))
                {
                    return _cachedResourcesByType[resourceType];
                }
            }

            // Build resources for this specific type
            var resources = BuildResourceTreeFromDirectoryByType(basePath, resourceType);

            lock (_lock)
            {
                _cachedResourcesByType[resourceType] = resources;
            }

            return resources;
        }

        /// <summary>
        /// Build resource tree from directory structure filtered by type
        /// </summary>
        private static List<GameResourceItem> BuildResourceTreeFromDirectoryByType(string basePath, GameResourceType resourceType)
        {
            var rootItems = new Dictionary<string, GameResourceItem>();
            var itemsLock = new object();

            // Get all files in the base directory recursively
            var allFiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);

            // Use parallel processing for faster file scanning
            System.Threading.Tasks.Parallel.ForEach(allFiles, filePath =>
            {
                try
                {
                    // Get relative path from base directory
                    var relativePath = Path.GetRelativePath(basePath, filePath);
                    var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    // Need at least 3 parts: category/subcategory/type/file
                    if (pathParts.Length < 3)
                        return;

                    // Last part is the filename
                    var fileName = pathParts[pathParts.Length - 1];
                    
                    // Second to last part is the type (e.g., AudioClip, Sprite, Texture2D)
                    var typeName = pathParts[pathParts.Length - 2];
                    
                    // Parse resource type from type name
                    var fileResourceType = ParseResourceType(typeName, fileName);

                    // Filter by requested type (skip if Folder or not matching)
                    if (resourceType != GameResourceType.Folder && fileResourceType != resourceType)
                        return;

                    // Everything before type becomes the folder path
                    var folderPath = string.Join("/", pathParts, 0, pathParts.Length - 2);

                    // Create file item
                    var fileItem = new GameResourceItem
                    {
                        Name = fileName,
                        Type = fileResourceType,
                        IsFolder = false,
                        Asset = filePath
                    };

                    // Thread-safe dictionary access
                    lock (itemsLock)
                    {
                        // Build folder hierarchy
                        if (!rootItems.ContainsKey(folderPath))
                        {
                            var folderItem = new GameResourceItem
                            {
                                Name = folderPath,
                                Type = GameResourceType.Folder,
                                IsFolder = true
                            };
                            rootItems[folderPath] = folderItem;
                        }

                        // Set parent and add to folder
                        fileItem.Parent = rootItems[folderPath];
                        rootItems[folderPath].Children.Add(fileItem);
                    }
                }
                catch (Exception ex)
                {
                    // Skip files that cause errors
                    DebugHelper.Error($"Error processing file {filePath}: {ex.Message}");
                }
            });

            // Convert flat dictionary to hierarchical tree
            return BuildHierarchicalTree(rootItems);
        }

        /// <summary>
        /// Build hierarchical tree from flat folder dictionary
        /// </summary>
        private static List<GameResourceItem> BuildHierarchicalTree(Dictionary<string, GameResourceItem> flatFolders)
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
                        // Create parent folder if it doesn't exist
                        var parentItem = CreateFolderChain(parentPath, folderCache, rootItems);
                        folderCache[parentPath] = parentItem;
                    }

                    var parent = folderCache[parentPath];
                    
                    // Update item name to show only the last part
                    item.Name = parts[parts.Length - 1];
                    item.Parent = parent;
                    parent.Children.Add(item);
                    folderCache[path] = item;
                }
            }

            return rootItems;
        }

        /// <summary>
        /// Create folder chain for nested paths
        /// </summary>
        private static GameResourceItem CreateFolderChain(string path, Dictionary<string, GameResourceItem> cache, List<GameResourceItem> rootItems)
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
        /// Parse resource type from type name and file extension
        /// </summary>
        private static GameResourceType ParseResourceType(string typeName, string fileName)
        {
            // Check by type name first
            if (typeName.Equals("AudioClip", StringComparison.OrdinalIgnoreCase))
                return GameResourceType.AudioClip;
            
            if (typeName.Equals("Sprite", StringComparison.OrdinalIgnoreCase))
                return GameResourceType.Sprite;
            
            if (typeName.Equals("Texture2D", StringComparison.OrdinalIgnoreCase))
                return GameResourceType.Texture2D;
            
            // if (typeName.Equals("TextAsset", StringComparison.OrdinalIgnoreCase))
            //     return GameResourceType.TextAsset;

            // Fallback to file extension
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            
            if (ext == ".wav" || ext == ".mp3" || ext == ".ogg")
                return GameResourceType.AudioClip;
            
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                return GameResourceType.Sprite;
            
            // if (ext == ".txt" || ext == ".json" || ext == ".xml")
            //     return GameResourceType.TextAsset;

            return GameResourceType.Other;
        }

        /// <summary>
        /// Apply max items per type limit to resource tree
        /// </summary>
        public static List<GameResourceItem> ApplyMaxPerType(List<GameResourceItem> items, int maxPerType)
        {
            if (maxPerType <= 0)
                return items;

            var typeCounts = new Dictionary<GameResourceType, int>();
            var result = new List<GameResourceItem>();

            void ProcessItems(List<GameResourceItem> source, List<GameResourceItem> target)
            {
                foreach (var item in source)
                {
                    if (item.IsFolder)
                    {
                        var folderCopy = new GameResourceItem
                        {
                            Name = item.Name,
                            Type = item.Type,
                            IsFolder = true,
                            Asset = item.Asset,
                            Parent = item.Parent
                        };

                        var childrenList = new List<GameResourceItem>();
                        ProcessItems(item.Children.ToList(), childrenList);
                        
                        foreach (var child in childrenList)
                        {
                            folderCopy.Children.Add(child);
                        }
                        
                        if (folderCopy.Children.Any())
                        {
                            target.Add(folderCopy);
                        }
                    }
                    else
                    {
                        if (!typeCounts.ContainsKey(item.Type))
                            typeCounts[item.Type] = 0;

                        if (typeCounts[item.Type] < maxPerType)
                        {
                            typeCounts[item.Type]++;
                            target.Add(item);
                        }
                    }
                }
            }

            ProcessItems(items, result);
            return result;
        }

        /// <summary>
        /// Get resource count by type
        /// </summary>
        public static Dictionary<GameResourceType, int> GetResourceCount(List<GameResourceItem> items)
        {
            var counts = new Dictionary<GameResourceType, int>();

            void CountItems(List<GameResourceItem> currentItems)
            {
                foreach (var item in currentItems)
                {
                    if (!item.IsFolder)
                    {
                        if (!counts.ContainsKey(item.Type))
                        {
                            counts[item.Type] = 0;
                        }
                        counts[item.Type]++;
                    }

                    if (item.Children.Any())
                    {
                        CountItems(item.Children.ToList());
                    }
                }
            }

            CountItems(items);
            return counts;
        }
    }
}
