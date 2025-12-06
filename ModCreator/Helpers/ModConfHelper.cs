using ModCreator.Models;
using System.Collections.Generic;
using System.Linq;

namespace ModCreator.Helpers
{
    public static class ModConfHelper
    {
        private static List<RegularPattern> _cachedPatterns;
        private static Dictionary<string, ModConfElement> _cachedElements;
        private static Dictionary<string, List<ModConfValue>> _cachedValues;

        /// <summary>
        /// Load all patterns from embedded resource (cached)
        /// </summary>
        public static List<RegularPattern> LoadPatterns(bool forceReload = false)
        {
            if (!forceReload && _cachedPatterns != null)
                return _cachedPatterns;

            _cachedPatterns = ResourceHelper.ReadEmbeddedResource<List<RegularPattern>>("ModCreator.Resources.modconf-patterns.json");
            return _cachedPatterns;
        }

        /// <summary>
        /// Load all elements from embedded resource (cached)
        /// </summary>
        public static Dictionary<string, ModConfElement> LoadElements(bool forceReload = false)
        {
            if (!forceReload && _cachedElements != null)
                return _cachedElements;

            var elements = ResourceHelper.ReadEmbeddedResource<List<ModConfElement>>("ModCreator.Resources.modconfs.json");
            _cachedElements = new Dictionary<string, ModConfElement>();
            
            foreach (var element in elements)
                _cachedElements[element.Name] = element;

            return _cachedElements;
        }

        /// <summary>
        /// Load all value groups from embedded resource (cached)
        /// </summary>
        public static Dictionary<string, List<ModConfValue>> LoadValues(bool forceReload = false)
        {
            if (!forceReload && _cachedValues != null)
                return _cachedValues;

            var valueGroups = ResourceHelper.ReadEmbeddedResource<List<ModConfValueGroup>>("ModCreator.Resources.modconf-values.json");
            _cachedValues = new Dictionary<string, List<ModConfValue>>();
            
            foreach (var group in valueGroups)
                _cachedValues[group.Name] = group.Values;

            return _cachedValues;
        }

        /// <summary>
        /// Get a specific element by name
        /// </summary>
        public static ModConfElement GetElement(string name)
        {
            var elements = LoadElements();
            return elements.TryGetValue(name, out var element) ? element : null;
        }

        /// <summary>
        /// Get values for a specific group
        /// </summary>
        public static List<ModConfValue> GetValues(string groupName)
        {
            var values = LoadValues();
            return values.TryGetValue(groupName, out var valueList) ? valueList : new List<ModConfValue>();
        }

        /// <summary>
        /// Get all patterns filtered by search text
        /// </summary>
        public static List<RegularPattern> GetFilteredPatterns(string searchText = null)
        {
            var patterns = LoadPatterns();
            
            if (string.IsNullOrWhiteSpace(searchText))
                return patterns.OrderBy(p => p.Name).ToList();

            return patterns
                .Where(p => p.Name.Contains(searchText, System.StringComparison.OrdinalIgnoreCase) 
                    || p.Description.Contains(searchText, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Name)
                .ToList();
        }

        /// <summary>
        /// Clear all caches (useful for testing or reloading)
        /// </summary>
        public static void ClearCache()
        {
            _cachedPatterns = null;
            _cachedElements = null;
            _cachedValues = null;
        }

        /// <summary>
        /// Get elements grouped by file name
        /// </summary>
        public static Dictionary<string, List<ModConfElement>> GetElementsByFileName()
        {
            var elements = LoadElements();
            return elements.Values
                .GroupBy(e => e.FileName)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
