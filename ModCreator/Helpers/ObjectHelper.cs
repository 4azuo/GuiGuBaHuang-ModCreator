using ModCreator.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for object operations
    /// </summary>
    public static class ObjectHelper
    {
        public static readonly Newtonsoft.Json.JsonSerializerSettings CLONE_JSON_SETTINGS = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
            PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
        };

        public static Type[] IgnoredTypes { get; } = 
        {
            typeof(AutoNotifiableObject),
            typeof(HistorableObject),
        };

        public static T Clone<T>(this T obj)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(obj, CLONE_JSON_SETTINGS), CLONE_JSON_SETTINGS);
        }

        public static void Map<T>(T src, T dest)
        {
            Map(src, dest, typeof(T));
        }

        public static void Map(object src, object dest, System.Type objType)
        {
            foreach (var p in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanWrite || !p.CanRead || IgnoredTypes.Contains(p.DeclaringType))
                    continue;
                var srcValue = p.GetValue(src);
                if (srcValue != null)
                    p.SetValue(dest, srcValue);
            }
        }

        public static void MapBySourceProp(object src, object dest)
        {
            foreach (var p1 in src.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var p2 = dest.GetType().GetProperty(p1.Name, BindingFlags.Public | BindingFlags.Instance);
                if (p2 == null || !p2.CanWrite || !p1.CanRead || IgnoredTypes.Contains(p2.DeclaringType))
                    continue;
                var srcValue = p1.GetValue(src);
                if (srcValue != null)
                    p2.SetValue(dest, srcValue);
            }
        }

        public static void MapByDestProp(object src, object dest)
        {
            foreach (var p1 in dest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var p2 = src.GetType().GetProperty(p1.Name, BindingFlags.Public | BindingFlags.Instance);
                if (p2 == null || !p1.CanWrite || !p2.CanRead || IgnoredTypes.Contains(p1.DeclaringType))
                    continue;
                var srcValue = p2.GetValue(src);
                if (srcValue != null)
                    p1.SetValue(dest, srcValue);
            }
        }

        public static FieldInfo GetField(this object obj, string fieldNm)
        {
            return obj.GetType().GetField(fieldNm);
        }

        public static PropertyInfo GetProperty(this object obj, string fieldNm)
        {
            return obj.GetType().GetProperty(fieldNm);
        }

        public static object GetValueUnsafe(this object obj, string fieldNm)
        {
            return obj.GetType().GetProperty(fieldNm).GetValue(obj);
        }

        public static object GetValue(this object obj, string fieldNm, bool ignorePropertyNotFoundError = false)
        {
            var prop = obj.GetType().GetProperty(fieldNm);
            if (prop == null)
            {
                if (ignorePropertyNotFoundError)
                    return null;
                throw new NullReferenceException();
            }
            return prop.GetValue(obj);
        }

        public static void SetValueUnsafe(this object obj, string fieldNm, object newValue)
        {
            obj.GetType().GetProperty(fieldNm).SetValue(obj, newValue);
        }

        public static void SetValue(this object obj, string fieldNm, object newValue, bool ignorePropertyNotFoundError = false, Func<Type, object> customParser = null)
        {
            var prop = obj.GetType().GetProperty(fieldNm);
            if (prop == null)
            {
                if (ignorePropertyNotFoundError)
                    return;
                throw new NullReferenceException();
            }
            var type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
            if (ParseHelper.TryParseUnknown(newValue, type, out object parsedValue))
            {
                prop.SetValue(obj, parsedValue);
            }
            else
            {
                if (customParser == null)
                {
                    //ignore
                }
                else
                {
                    prop.SetValue(obj, customParser(type));
                }
            }
        }

        public static bool IsDeclaredMethod(this object obj, string medName)
        {
            return obj?.GetType()?.GetMethod(medName)?.DeclaringType == obj.GetType();
        }

        public static string GetBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        public static FieldInfo GetBackingField(object obj, string propertyName)
        {
            return obj.GetType().GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static void SetBackingField(object obj, string propertyName, object value)
        {
            GetBackingField(obj, propertyName).SetValue(obj, value);
        }

        /// <summary>
        /// Compare all public properties between two objects of the same type
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="trackedTypes">Types to track for deep comparison</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in comparison</param>
        /// <returns>True if all properties are equal, false otherwise</returns>
        public static bool ArePropertiesEqual<T>(T obj1, T obj2, Type[] trackedTypes, params string[] ignoreProperties) where T : class
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;
            if (ReferenceEquals(obj1, obj2)) return true;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && (trackedTypes == null || trackedTypes.Contains(p.DeclaringType)) && !ignoreProperties.Contains(p.Name) && !IgnoredTypes.Contains(p.DeclaringType));

            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!AreValuesEqual(value1, value2, trackedTypes, ignoreProperties))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get a dictionary of property differences between two objects
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="trackedTypes">Types to track for deep comparison</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in comparison</param>
        /// <returns>Dictionary with property names as keys and tuple of (value1, value2) as values</returns>
        public static Dictionary<string, (object value1, object value2)> GetPropertyDifferences<T>(T obj1, T obj2, Type[] trackedTypes, params string[] ignoreProperties) where T : class
        {
            var differences = new Dictionary<string, (object, object)>();

            if (obj1 == null || obj2 == null) return differences;
            if (ReferenceEquals(obj1, obj2)) return differences;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && (trackedTypes == null || trackedTypes.Contains(p.DeclaringType)) && !ignoreProperties.Contains(p.Name) && !IgnoredTypes.Contains(p.DeclaringType));

            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!AreValuesEqual(value1, value2, trackedTypes, ignoreProperties))
                {
                    differences[property.Name] = (value1, value2);
                }
            }

            return differences;
        }

        /// <summary>
        /// Copy all public properties from source to target object
        /// </summary>
        /// <typeparam name="T">Type of objects</typeparam>
        /// <param name="source">Source object to copy from</param>
        /// <param name="target">Target object to copy to</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in copy</param>
        public static void CopyProperties<T>(T source, T target, params string[] ignoreProperties) where T : class
        {
            if (source == null || target == null) return;
            if (ReferenceEquals(source, target)) return;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && !ignoreProperties.Contains(p.Name) && !IgnoredTypes.Contains(p.DeclaringType));

            foreach (var property in properties)
            {
                var value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }

        /// <summary>
        /// Copy properties from source to target, only copying specified properties
        /// </summary>
        /// <typeparam name="T">Type of objects</typeparam>
        /// <param name="source">Source object to copy from</param>
        /// <param name="target">Target object to copy to</param>
        /// <param name="propertiesToCopy">List of property names to copy</param>
        public static void CopyPropertiesSelective<T>(T source, T target, params string[] propertiesToCopy) where T : class
        {
            if (source == null || target == null) return;
            if (ReferenceEquals(source, target)) return;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && propertiesToCopy.Contains(p.Name));

            foreach (var property in properties)
            {
                var value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }

        /// <summary>
        /// Deep clone an object using reflection (for simple objects without circular references)
        /// </summary>
        /// <typeparam name="T">Type of object to clone</typeparam>
        /// <param name="source">Source object to clone</param>
        /// <returns>Cloned object</returns>
        public static T DeepClone<T>(T source) where T : class, new()
        {
            if (source == null) return null;

            var clone = new T();
            CopyProperties(source, clone);
            return clone;
        }

        /// <summary>
        /// Compare two values for equality, handling nulls and collections
        /// </summary>
        public static bool AreValuesEqual(object value1, object value2, Type[] trackedTypes, params string[] ignoreProperties)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Handle strings separately to avoid treating them as IEnumerable
            if (value1 is string || value2 is string)
            {
                return Equals(value1, value2);
            }

            // Handle collections
            if (value1 is System.Collections.IEnumerable enum1 && value2 is System.Collections.IEnumerable enum2)
            {
                var list1 = enum1.Cast<object>().ToList();
                var list2 = enum2.Cast<object>().ToList();

                if (list1.Count != list2.Count) return false;

                for (int i = 0; i < list1.Count; i++)
                {
                    if (!ArePropertiesEqual(list1[i], list2[i], trackedTypes, ignoreProperties))
                        return false;
                }

                return true;
            }

            // Handle complex objects by comparing their properties
            var type1 = value1.GetType();
            var type2 = value2.GetType();

            if (type1 != type2) return false;

            // If it's a value type or primitive, use Equals
            if (type1.IsValueType || type1.IsPrimitive || type1.IsEnum)
            {
                return Equals(value1, value2);
            }
            else
            {
                return ArePropertiesEqual(value1, value2, trackedTypes, ignoreProperties);
            }
        }

        /// <summary>
        /// Get list of property names that have different values between two objects
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="obj1">First object</param>
        /// <param name="obj2">Second object</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore in comparison</param>
        /// <returns>List of property names with different values</returns>
        public static List<string> GetChangedPropertyNames<T>(T obj1, T obj2, Type[] trackedTypes, params string[] ignoreProperties) where T : class
        {
            var differences = GetPropertyDifferences(obj1, obj2, trackedTypes, ignoreProperties);
            return differences.Keys.ToList();
        }

        /// <summary>
        /// Calculate hash code for an object, supporting complex types and collections
        /// </summary>
        /// <param name="obj">The object to calculate hash code for</param>
        /// <param name="trackedTypes">Types to track for deep hash calculation</param>
        /// <param name="ignoreProperties">Optional list of property names to ignore</param>
        /// <returns>Combined hash code</returns>
        public static int GetObjectHashCode(object obj, Type[] trackedTypes, params string[] ignoreProperties)
        {
            var visited = new HashSet<object>(new ReferenceEqualityComparer());
            return GetObjectHashCodeInternal(obj, trackedTypes, ignoreProperties, visited);
        }

        /// <summary>
        /// Internal method to calculate hash code with circular reference detection
        /// </summary>
        private static int GetObjectHashCodeInternal(object obj, Type[] trackedTypes, string[] ignoreProperties, HashSet<object> visited)
        {
            if (obj == null) return 0;

            var type = obj.GetType();

            // Handle strings separately to avoid treating them as IEnumerable
            if (obj is string)
            {
                return obj.GetHashCode();
            }

            // Handle primitives and value types
            if (type.IsValueType || type.IsPrimitive || type.IsEnum)
            {
                return obj.GetHashCode();
            }

            // Check for circular references
            if (!visited.Add(obj))
            {
                // Already visited, return a constant to avoid infinite recursion
                return 0;
            }

            // Handle collections
            if (obj is System.Collections.IEnumerable enumerable)
            {
                unchecked
                {
                    int hash = 17;
                    foreach (var item in enumerable)
                    {
                        hash = hash * 31 + GetObjectHashCodeInternal(item, trackedTypes, ignoreProperties, visited);
                    }
                    return hash;
                }
            }

            // Handle complex objects - only if tracked
            if (trackedTypes != null && !trackedTypes.Contains(type))
            {
                // Non-tracked types, use default GetHashCode
                return obj.GetHashCode();
            }

            // For tracked complex objects, calculate hash from properties
            unchecked
            {
                int hash = 17;
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && 
                                !ignoreProperties.Contains(p.Name) && 
                                !IgnoredTypes.Contains(p.DeclaringType));

                foreach (var property in properties)
                {
                    var value = property.GetValue(obj);
                    hash = hash * 31 + GetObjectHashCodeInternal(value, trackedTypes, ignoreProperties, visited);
                }

                return hash;
            }
        }

        /// <summary>
        /// Reference equality comparer for HashSet to detect circular references
        /// </summary>
        private class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
