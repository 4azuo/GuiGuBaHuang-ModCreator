using ModCreator.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ModCreator.Helpers
{
    public static class PatternHelper
    {
        private static readonly Regex AutoGenPlaceholderRegex = new Regex(@"\{([^}]+)\}", RegexOptions.Compiled);
        private static readonly Regex AttributeRegex = new Regex(@"\[(Disabled|ReadOnly|Required|RefDocs:=.+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetKeyAfterFirstDot(string value)
        {
            return value.Contains('.') ? value.Substring(value.IndexOf('.') + 1) : value;
        }

        /// <summary>
        /// Parse element attributes from element name
        /// Format: ElementName[Disabled][ReadOnly][Required][RefDocs:filename]:format
        /// </summary>
        public static dynamic ParseElementAttributes(string fullElementName)
        {
            var elementName = fullElementName;
            var disabled = false;
            var readOnly = false;
            var required = false;
            string refDocs = null;
            string format = null;

            // Remove format part first (after :)
            if (elementName.Contains("::"))
            {
                var parts = elementName.Split("::");
                elementName = parts[0];
                format = parts[1];
            }

            // Find and remove all attribute matches
            var matches = AttributeRegex.Matches(elementName);
            foreach (Match match in matches)
            {
                var attr = match.Groups[1].Value;
                var attrLower = attr.ToLower();
                if (attrLower == "disabled")
                {
                    disabled = true;
                }
                else if (attrLower == "readonly")
                {
                    readOnly = true;
                }
                else if (attrLower == "required")
                {
                    required = true;
                }
                else if (attrLower.StartsWith("refdocs:="))
                {
                    refDocs = attr.Substring(9); // Extract filename after "RefDocs:"
                }
            }

            // Remove all attribute brackets from element name
            elementName = AttributeRegex.Replace(elementName, string.Empty);

            return new
            {
                FullElementName = fullElementName,
                ElementName = elementName,
                Disabled = disabled,
                ReadOnly = readOnly,
                Required = required,
                RefDocs = refDocs,
                Format = format
            };
        }

        public static string ProcessAutoGenValue(string autoGenPattern, Dictionary<string, string> rowData)
        {
            if (string.IsNullOrEmpty(autoGenPattern))
                return null;

            return AutoGenPlaceholderRegex.Replace(autoGenPattern, match =>
            {
                var placeholder = match.Groups[1].Value;
                var actualKey = GetKeyAfterFirstDot(placeholder);
                if (rowData.TryGetValue(actualKey, out var value)) return value;
                return match.Value;
            });
        }

        public static string ProcessCompositeValue(PatternElement element, Dictionary<string, string> rowData)
        {
            if (element.Type != "composite" || element.SubElements == null || element.SubElements.Count == 0)
                return null;

            var parts = new List<string>();
            foreach (var subElement in element.SubElements)
            {
                var subValue = rowData.ContainsKey(subElement.Name) ? rowData[subElement.Name] : string.Empty;
                if (!string.IsNullOrWhiteSpace(subValue))
                    parts.Add(subValue);
            }

            return parts.Count > 0 ? string.Join(element.Separator ?? "_", parts) : null;
        }

        public static void DecomposeCompositeValue(PatternElement element, string compositeValue, Dictionary<string, string> rowData)
        {
            if (element.Type != "composite" || element.SubElements == null || element.SubElements.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(compositeValue))
                return;

            var separator = element.Separator ?? "_";
            var parts = compositeValue.Split(new[] { separator }, StringSplitOptions.None);

            for (int i = 0; i < Math.Min(parts.Length, element.SubElements.Count); i++)
            {
                var subElement = element.SubElements[i];
                rowData[subElement.Name] = parts[i];
            }
        }
    }
}
