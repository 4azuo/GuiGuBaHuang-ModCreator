namespace ModCreator.Models
{
    /// <summary>
    /// Represents a folder item for game resource filtering.
    /// </summary>
    public class GameResourceFolderItem
    {
        public string FolderPath { get; set; }
        public string DisplayName => FolderPath;
        public string Value => FolderPath;
        public bool IsSelected { get; set; }
    }
}
