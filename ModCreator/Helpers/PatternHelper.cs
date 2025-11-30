using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ModCreator.Helpers
{
    public static class PatternHelper
    {
        private static readonly Regex AutoGenPlaceholderRegex = new Regex(@"\{([^}]+)\}", RegexOptions.Compiled);

        public static string ProcessAutoGenValue(string autoGenPattern, Dictionary<string, string> rowData)
        {
            if (string.IsNullOrEmpty(autoGenPattern))
                return null;

            return AutoGenPlaceholderRegex.Replace(autoGenPattern, match =>
            {
                var placeholder = match.Groups[1].Value;
                var actualKey = placeholder.Contains(".") ? placeholder.Split('.')[1] : placeholder;
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
