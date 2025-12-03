using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ModCreator.Helpers
{
    public static class ModEventHelper
    {
        private static List<Models.EventActionBase> _cachedEvents;
        private static List<Models.EventActionBase> _cachedActions;

        /// <summary>
        /// Check if a member has a specific attribute
        /// </summary>
        public static bool HasAttribute(MemberInfo member, string attributeFullName)
        {
            return member.GetCustomAttributesData()
                .Any(a => a.AttributeType.FullName == attributeFullName);
        }

        /// <summary>
        /// Get attribute value from a member
        /// </summary>
        public static object GetAttributeValue(MemberInfo member, string attributeFullName, int argumentIndex = 0)
        {
            var attr = member.GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType.FullName == attributeFullName);
            
            if (attr != null && attr.ConstructorArguments.Count > argumentIndex)
            {
                return attr.ConstructorArguments[argumentIndex].Value;
            }
            
            return null;
        }

        /// <summary>
        /// Format type name for display (handle generic types)
        /// </summary>
        public static string FormatTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            try
            {
                var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
                var genericArgs = string.Join(", ", type.GetGenericArguments().Select(t => FormatTypeName(t)));
                return $"{genericTypeName}<{genericArgs}>";
            }
            catch
            {
                return type.Name;
            }
        }

        /// <summary>
        /// Execute action within MetadataLoadContext scope
        /// </summary>
        public static void WithMetadataLoadContext(Action<Assembly> action, Action<Exception> catcher = null)
        {
            try
            {
                var modLibPath = Path.Combine(Constants.ResourcesDir, "ModProject_0hKMNX", "ModProject", "ModCode", "ModMain", "CodeArtifacts", "ModLib.dll");
                if (!File.Exists(modLibPath))
                    return;
                var dll = Path.Combine(Constants.ResourcesDir, "ModProject_0hKMNX", "ModProject", "ModCode", "ModMain", "dll", "Assembly-CSharp.dll");
                if (!File.Exists(dll))
                    return;

                var paths = new List<string>();
                
                // Add runtime assemblies
                paths.AddRange(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));

                // Add game folder DLLs
                paths.AddRange(Directory.GetFiles(Constants.GameFolderPath, "*.dll", SearchOption.AllDirectories));

                // Add project DLLs
                paths.Add(dll);
                paths.Add(modLibPath);
                
                // Distinct by filename to avoid duplicates
                var distinctPaths = paths
                    .GroupBy(p => Path.GetFileName(p))
                    .Select(g => g.First())
                    .ToList();
                
                var resolver = new PathAssemblyResolver(distinctPaths);
                using var mlc = new MetadataLoadContext(resolver);
                var assembly = mlc.LoadFromAssemblyPath(modLibPath);
                
                action(assembly);
            }
            catch (Exception ex)
            {
                catcher?.Invoke(ex);
            }
        }

        /// <summary>
        /// Load ModEvent methods from ModLib.dll assembly
        /// </summary>
        public static List<Models.EventActionBase> LoadModEventMethodsFromAssembly(bool forceReload = false)
        {
            if (!forceReload && _cachedEvents != null)
                return _cachedEvents.Clone();

            var items = LoadMethodsFromAssembly(
                typeFilter: t => t.FullName == "ModLib.Mod.ModEvent",
                methodFilter: m => m.IsVirtual && !m.IsSpecialName,
                categoryAttribute: "ModLib.Attributes.EventCatAttribute",
                ignoreAttribute: "ModLib.Attributes.EventCatIgnAttribute",
                bindingFlags: BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                itemFactory: (typeName, method, category, code, parameters) => new Models.EventActionBase
                {
                    Category = category,
                    Name = method.Name,
                    DisplayName = method.Name,
                    Description = $"ModEvent method: {method.Name}",
                    Code = code,
                    Parameters = parameters.Select(p => new Models.ParameterInfo
                    {
                        Type = FormatTypeName(p.ParameterType),
                        Name = p.Name,
                        IsOptional = p.IsOptional,
                        DefaultValue = p.HasDefaultValue ? (p.RawDefaultValue?.ToString() ?? "null") : string.Empty
                    }).ToList(),
                    Return = FormatTypeName(method.ReturnType),
                    HasBody = false,
                    IsHidden = false,
                    IsCanAddChild = false,
                    SubItems = []
                });

            _cachedEvents = items;
            return items;
        }

        /// <summary>
        /// Load Action methods from ModLib.Helper.* classes in ModLib.dll assembly
        /// </summary>
        public static List<Models.EventActionBase> LoadModActionMethodsFromAssembly(bool forceReload = false)
        {
            if (!forceReload && _cachedActions != null)
                return _cachedActions.Clone();

            var items = LoadMethodsFromAssembly(
                typeFilter: t => t.Namespace != null && t.Namespace.StartsWith("ModLib.Helper"),
                methodFilter: m => !m.IsSpecialName,
                categoryAttribute: null,
                ignoreAttribute: "ModLib.Attributes.ActionCatIgnAttribute",
                bindingFlags: BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                itemFactory: (typeName, method, category, code, parameters) => new Models.EventActionBase
                {
                    Category = typeName,
                    Name = $"{typeName}.{method.Name}",
                    DisplayName = $"{typeName}.{method.Name}",
                    Description = $"Helper method from {typeName}",
                    Code = code,
                    Parameters = parameters.Select(p => new Models.ParameterInfo
                    {
                        Type = FormatTypeName(p.ParameterType),
                        Name = p.Name,
                        IsOptional = p.IsOptional,
                        DefaultValue = p.HasDefaultValue ? (p.RawDefaultValue?.ToString() ?? "null") : string.Empty
                    }).ToList(),
                    Return = FormatTypeName(method.ReturnType),
                    HasBody = false,
                    IsHidden = false,
                    IsCanAddChild = false,
                    SubItems = []
                });

            _cachedActions = items;
            return items;
        }

        /// <summary>
        /// Generic method to load methods from assembly based on filters and attributes
        /// </summary>
        public static List<T> LoadMethodsFromAssembly<T>(
            Func<Type, bool> typeFilter,
            Func<MethodInfo, bool> methodFilter,
            string categoryAttribute,
            string ignoreAttribute,
            BindingFlags bindingFlags,
            Func<string, MethodInfo, string, string, ParameterInfo[], T> itemFactory) where T : Models.EventActionBase
        {
            var items = new List<T>();

            WithMetadataLoadContext(assembly =>
            {
                // Get all types matching the filter
                var types = assembly.GetTypes().Where(typeFilter).ToList();

                foreach (var type in types)
                {
                    // Skip if type has ignore attribute
                    if (!string.IsNullOrEmpty(ignoreAttribute) && HasAttribute(type, ignoreAttribute))
                        continue;

                    var methods = type.GetMethods(bindingFlags)
                        .Where(methodFilter)
                        .ToList();

                    foreach (var method in methods)
                    {
                        // Skip if method has ignore attribute
                        if (!string.IsNullOrEmpty(ignoreAttribute) && HasAttribute(method, ignoreAttribute))
                            continue;

                        // Skip obsolete methods
                        if (HasAttribute(method, "System.ObsoleteAttribute"))
                            continue;

                        var parameters = method.GetParameters();
                        var code = $"{FormatTypeName(method.ReturnType)} {method.Name}({string.Join(", ", parameters.Select(p => $"{FormatTypeName(p.ParameterType)} {p.Name}"))})";

                        // Get category from attribute or use default
                        var category = string.IsNullOrEmpty(categoryAttribute)
                            ? "Others"
                            : GetAttributeValue(method, categoryAttribute)?.ToString() ?? "Others";

                        items.Add(itemFactory(type.Name, method, category, code, parameters));
                    }
                }
            }, (e) =>
            {
                DebugHelper.Error(e.Message);
            });

            return items;
        }
    }
}
