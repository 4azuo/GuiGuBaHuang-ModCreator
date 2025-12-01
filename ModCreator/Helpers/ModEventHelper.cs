using ModCreator.Models;
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
        /// <summary>
        /// Execute action within MetadataLoadContext scope
        /// </summary>
        private static void WithMetadataLoadContext(Action<Assembly> action)
        {
            try
            {
                var modLibPath = Path.Combine(Constants.ResourcesDir, "ModProject_0hKMNX", "ModProject", "ModCode", "ModMain", "CodeArtifacts", "ModLib.dll");
                if (!File.Exists(modLibPath))
                    return;
                var dll = Path.Combine(Constants.ResourcesDir, "ModProject_0hKMNX", "ModProject", "ModCode", "ModMain", "dll", "Assembly-CSharp.dll");
                if (!File.Exists(modLibPath))
                    return;

                var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
                var paths = new List<string>(runtimeAssemblies) { dll, modLibPath };
                var resolver = new PathAssemblyResolver(paths);
                using var mlc = new MetadataLoadContext(resolver);
                var assembly = mlc.LoadFromAssemblyPath(modLibPath);
                
                action(assembly);
            }
            catch
            {
                // Silently fail if assembly cannot be loaded
            }
        }

        /// <summary>
        /// Load ModEvent methods from ModLib.dll assembly
        /// </summary>
        public static List<Models.EventInfo> LoadModEventMethodsFromAssembly()
        {
            var events = new List<Models.EventInfo>();
            
            WithMetadataLoadContext(assembly =>
            {
                var modEventType = assembly.GetType("ModLib.Mod.ModEvent");
                if (modEventType == null)
                    return;
                var atrEventCat = assembly.GetType("ModLib.Attributes.EventCatAttribute");
                if (atrEventCat == null)
                    return;

                var methods = modEventType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.IsVirtual && !m.IsSpecialName).ToList();

                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    var code = $"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))})";

                    // Get category from EventCatAttribute
                    var category = "Others";
                    var eventCatAttr = method.GetCustomAttributesData()
                        .FirstOrDefault(a => a.AttributeType.FullName == "ModLib.Attributes.EventCatAttribute");
                    if (eventCatAttr != null && eventCatAttr.ConstructorArguments.Count > 0)
                    {
                        category = eventCatAttr.ConstructorArguments[0].Value?.ToString() ?? "Others";
                    }

                    events.Add(new Models.EventInfo
                    {
                        Category = category,
                        Name = method.Name,
                        DisplayName = method.Name,
                        Description = $"ModEvent method: {method.Name}",
                        Code = code
                    });
                }
            });

            return events;
        }
    }
}
