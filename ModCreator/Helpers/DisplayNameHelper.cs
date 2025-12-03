using ModCreator.Enums;
using ModCreator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ModCreator.Helpers
{
    public static class DisplayNameHelper
    {
        private static readonly Regex ParameterPlaceholderRegex = new(@"\{(\d+)\}");

        public static string BuildNestedDisplayName(ModEventItemSelectValue value)
        {
            return BuildNestedDisplayName(value, new HashSet<EventActionBase>());
        }

        private static string BuildNestedDisplayName(ModEventItemSelectValue value, HashSet<EventActionBase> visited)
        {
            if (value == null)
                return string.Empty;

            if (value.SelectType == ModEventSelectType.Variable)
                return value.Name;

            if (value.SelectType == ModEventSelectType.OptionalValue)
                return value.OptionalValue;

            return BuildNestedDisplayName(value.SelectedEventAction, visited);
        }

        public static string BuildNestedDisplayName(EventActionBase action)
        {
            return BuildNestedDisplayName(action, new HashSet<EventActionBase>());
        }

        private static string BuildNestedDisplayName(EventActionBase action, HashSet<EventActionBase> visited)
        {
            if (action == null || string.IsNullOrEmpty(action.DisplayName))
                return string.Empty;

            // Circular reference detection
            if (!visited.Add(action))
                return $"[Circular: {action.DisplayName}]";

            try
            {
                var displayName = action.DisplayName;
                var matches = ParameterPlaceholderRegex.Matches(displayName);

                if (matches.Count == 0 || action.ParameterValues.Count == 0)
                    return displayName;

                int lastIndex = 0;
                var result = new StringBuilder();

                foreach (Match match in matches)
                {
                    result.Append(displayName.Substring(lastIndex, match.Index - lastIndex));

                    if (int.TryParse(match.Groups[1].Value, out int paramIndex) &&
                        action.ParameterValues.ContainsKey(paramIndex) &&
                        action.ParameterValues[paramIndex] != null)
                    {
                        var paramValue = action.ParameterValues[paramIndex];
                        var nestedDisplay = BuildNestedDisplayName(paramValue, visited);

                        var needsParentheses = paramValue.SelectType == ModEventSelectType.EventAction &&
                                                paramValue.SelectedEventAction?.ParameterValues?.Count > 0;

                        result.Append(needsParentheses ? $"({nestedDisplay})" : nestedDisplay);
                    }
                    else
                    {
                        result.Append(match.Value);
                    }

                    lastIndex = match.Index + match.Length;
                }

                result.Append(displayName.Substring(lastIndex));
                return result.ToString();
            }
            finally
            {
                visited.Remove(action);
            }
        }
    }
}
