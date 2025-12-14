using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModCreator.Helpers
{
    public static class EnumerableHelper
    {
        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void ReplaceWith<T>(this IList<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            collection.AddRange(items);
        }

        public static ObservableCollection<T> ToOC<T>(this IEnumerable<T> collection)
        {
            var oc = new ObservableCollection<T>();
            oc.AddRange(collection);
            return oc;
        }
    }
}
