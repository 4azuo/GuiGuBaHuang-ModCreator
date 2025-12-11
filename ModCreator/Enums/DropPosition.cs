namespace ModCreator.Enums
{
    /// <summary>
    /// Represents the position where an item will be dropped during drag and drop operations
    /// </summary>
    public enum DropPosition
    {
        /// <summary>
        /// Drop above the target item
        /// </summary>
        Above,

        /// <summary>
        /// Drop below the target item
        /// </summary>
        Below,

        /// <summary>
        /// Drop inside the target item as a child
        /// </summary>
        Inside
    }
}
