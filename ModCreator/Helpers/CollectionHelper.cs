using System.Collections.Generic;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper methods for collections
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Moves an item from one index to another in a list
        /// </summary>
        /// <typeparam name="T">Type of items in the list</typeparam>
        /// <param name="list">The list to modify</param>
        /// <param name="oldIndex">The current index of the item</param>
        /// <param name="newIndex">The target index for the item</param>
        public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            if (list == null)
                throw new System.ArgumentNullException(nameof(list));

            if (oldIndex < 0 || oldIndex >= list.Count)
                throw new System.ArgumentOutOfRangeException(nameof(oldIndex));

            if (newIndex < 0 || newIndex >= list.Count)
                throw new System.ArgumentOutOfRangeException(nameof(newIndex));

            if (oldIndex == newIndex)
                return;

            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
    }
}
