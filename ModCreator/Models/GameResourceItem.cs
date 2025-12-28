using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Enums;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    /// <summary>
    /// Game resource item for TreeView display
    /// </summary>
    [SetterAspect]
    public class GameResourceItem : AutoNotifiableObject
    {
        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full path in game (like "Assets/Texture/...")
        /// </summary>
        public string PathInGame { get; set; } = string.Empty;

        /// <summary>
        /// Extracted file path (for images)
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Resource type
        /// </summary>
        public GameResourceType Type { get; set; }

        /// <summary>
        /// Is this a folder?
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Child items (for folders)
        /// </summary>
        public ObservableCollection<GameResourceItem> Children { get; set; } = [];

        /// <summary>
        /// Parent folder item
        /// </summary>
        public GameResourceItem Parent { get; set; }

        /// <summary>
        /// Associated Unity asset object
        /// </summary>
        public object Asset { get; set; }

        /// <summary>
        /// Get typed asset
        /// </summary>
        public T GetAsset<T>() where T : class
        {
            return Asset as T;
        }
    }
}
