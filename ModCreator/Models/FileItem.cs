using ModCreator.Commons;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    /// <summary>
    /// File or folder item for TreeView display
    /// </summary>
    public class FileItem : AutoNotifiableObject
    {
        /// <summary>
        /// Display name (file or folder name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full file path
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Relative path from root
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Is this a folder?
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Child items (for folders)
        /// </summary>
        public ObservableCollection<FileItem> Children { get; set; } = [];

        /// <summary>
        /// Parent folder item
        /// </summary>
        public FileItem Parent { get; set; }

        /// <summary>
        /// File content (for files only)
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the content of the object.
        /// </summary>
        public object ObjectContent { get; set; }

        /// <summary>
        /// Retrieves the content of the object as the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the object content should be cast.</typeparam>
        /// <returns>The object content cast to the specified type <typeparamref name="T"/>.</returns>
        public T GetObjectContentAs<T>()
        {
            return (T)ObjectContent;
        }
    }
}
