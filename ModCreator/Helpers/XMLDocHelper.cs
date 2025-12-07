using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper to read and cache XML documentation from ModLib.xml
    /// </summary>
    public static class XMLDocHelper
    {
        private static Dictionary<string, string> _docCache;
        private static readonly object _lock = new object();
        private static bool _isLoaded = false;

        /// <summary>
        /// Load XML documentation from ModLib.xml and cache it
        /// </summary>
        private static void LoadDocumentation()
        {
            lock (_lock)
            {
                if (_isLoaded)
                    return;

                _docCache = new Dictionary<string, string>();

                try
                {
                    var doc = XDocument.Parse(ResourceHelper.ReadEmbeddedResource("ModCreator.Resources.ModLib.xml"));
                    var members = doc.Root?.Element("members")?.Elements("member");

                    if (members == null)
                    {
                        DebugHelper.Warning("No members found in XML documentation");
                        _isLoaded = true;
                        return;
                    }

                    foreach (var member in members)
                    {
                        var name = member.Attribute("name")?.Value;
                        if (string.IsNullOrEmpty(name))
                            continue;

                        // Get summary element
                        var summary = member.Element("summary")?.Value?.Trim();
                        
                        // Get remarks if available
                        var remarks = member.Element("remarks")?.Value?.Trim();
                        
                        // Get parameters if available
                        var parameters = member.Elements("param")
                            .Select(p => new
                            {
                                Name = p.Attribute("name")?.Value,
                                Description = p.Value?.Trim()
                            })
                            .Where(p => !string.IsNullOrEmpty(p.Name))
                            .ToList();

                        // Get return value if available
                        var returns = member.Element("returns")?.Value?.Trim();

                        // Build documentation string
                        var docText = summary ?? string.Empty;
                        
                        if (!string.IsNullOrEmpty(remarks))
                            docText += $"\n\n{remarks}";
                        
                        if (parameters.Any())
                        {
                            docText += "\n\nParameters:";
                            foreach (var param in parameters)
                            {
                                docText += $"\n  {param.Name}: {param.Description}";
                            }
                        }

                        if (!string.IsNullOrEmpty(returns))
                            docText += $"\n\nReturns: {returns}";

                        _docCache[name] = docText;
                    }

                    _isLoaded = true;
                    DebugHelper.Info($"Loaded {_docCache.Count} documentation entries from ModLib.xml");
                }
                catch (Exception ex)
                {
                    DebugHelper.Error($"Failed to load XML documentation: {ex.Message}");
                    _isLoaded = true;
                }
            }
        }

        /// <summary>
        /// Get documentation for a type
        /// </summary>
        /// <param name="typeName">Full type name (e.g., "ModLib.Mod.ModEvent")</param>
        /// <returns>Documentation string or null if not found</returns>
        public static string GetTypeDocumentation(string typeName)
        {
            if (!_isLoaded)
                LoadDocumentation();

            var key = $"T:{typeName}";
            return _docCache.TryGetValue(key, out var doc) ? doc : null;
        }

        /// <summary>
        /// Get documentation for a method
        /// </summary>
        /// <param name="typeName">Full type name</param>
        /// <param name="methodName">Method name</param>
        /// <param name="parameterTypes">Parameter types (optional, for overload resolution)</param>
        /// <returns>Documentation string or null if not found</returns>
        public static string GetMethodDocumentation(string typeName, string methodName, string[] parameterTypes = null)
        {
            if (!_isLoaded)
                LoadDocumentation();

            // Try exact match first if parameters provided
            if (parameterTypes != null && parameterTypes.Length > 0)
            {
                var paramStr = string.Join(",", parameterTypes);
                var keyWithParams = $"M:{typeName}.{methodName}({paramStr})";
                if (_docCache.TryGetValue(keyWithParams, out var docWithParams))
                    return docWithParams;
            }

            // Try without parameters
            var key = $"M:{typeName}.{methodName}";
            if (_docCache.TryGetValue(key, out var doc))
                return doc;

            // Try to find any overload
            var prefix = $"M:{typeName}.{methodName}(";
            var match = _docCache.FirstOrDefault(kvp => kvp.Key.StartsWith(prefix));
            return match.Value;
        }

        /// <summary>
        /// Get documentation for a property
        /// </summary>
        /// <param name="typeName">Full type name</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Documentation string or null if not found</returns>
        public static string GetPropertyDocumentation(string typeName, string propertyName)
        {
            if (!_isLoaded)
                LoadDocumentation();

            var key = $"P:{typeName}.{propertyName}";
            return _docCache.TryGetValue(key, out var doc) ? doc : null;
        }

        /// <summary>
        /// Get documentation for a field
        /// </summary>
        /// <param name="typeName">Full type name</param>
        /// <param name="fieldName">Field name</param>
        /// <returns>Documentation string or null if not found</returns>
        public static string GetFieldDocumentation(string typeName, string fieldName)
        {
            if (!_isLoaded)
                LoadDocumentation();

            var key = $"F:{typeName}.{fieldName}";
            return _docCache.TryGetValue(key, out var doc) ? doc : null;
        }

        /// <summary>
        /// Get documentation for any member by full member name
        /// </summary>
        /// <param name="memberName">Full member name with prefix (e.g., "M:ModLib.Mod.ModEvent.OnOpenUIStart")</param>
        /// <returns>Documentation string or null if not found</returns>
        public static string GetDocumentation(string memberName)
        {
            if (!_isLoaded)
                LoadDocumentation();

            return _docCache.TryGetValue(memberName, out var doc) ? doc : null;
        }

        /// <summary>
        /// Search for documentation entries containing the search term
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>Dictionary of matching member names and their documentation</returns>
        public static Dictionary<string, string> SearchDocumentation(string searchTerm)
        {
            if (!_isLoaded)
                LoadDocumentation();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return new Dictionary<string, string>();

            return _docCache
                .Where(kvp => kvp.Key.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                             kvp.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Get all cached documentation entries
        /// </summary>
        /// <returns>Dictionary of all member names and their documentation</returns>
        public static Dictionary<string, string> GetAllDocumentation()
        {
            if (!_isLoaded)
                LoadDocumentation();

            return new Dictionary<string, string>(_docCache);
        }

        /// <summary>
        /// Clear the documentation cache and force reload on next access
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _docCache?.Clear();
                _docCache = null;
                _isLoaded = false;
            }
        }

        /// <summary>
        /// Check if documentation is loaded
        /// </summary>
        public static bool IsLoaded => _isLoaded;

        /// <summary>
        /// Get the number of cached documentation entries
        /// </summary>
        public static int CacheCount
        {
            get
            {
                if (!_isLoaded)
                    LoadDocumentation();
                return _docCache?.Count ?? 0;
            }
        }
    }
}
