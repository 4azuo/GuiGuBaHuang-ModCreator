using ModCreator.Attributes;
using ModCreator.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace ModCreator.Commons
{
    /// <summary>
    /// Provides a base class that implements <see cref="INotifyPropertyChanged"/> and supports automatic property
    /// change notification for derived objects.
    /// </summary>
    /// <remarks><para> <b>AutoNotifiableObject</b> simplifies the implementation of property change
    /// notification by automatically tracking and notifying changes to properties in derived classes. It manages
    /// property state, supports pausing and resuming notifications, and allows for custom notification methods to be
    /// invoked when properties change. </para> <para> Properties marked with <see cref="IgnoredPropertyAttribute"/> are
    /// excluded from notification. The class also maintains dictionaries to track previous property values and hash
    /// codes, which can be useful for advanced scenarios such as change tracking or undo functionality. </para> <para>
    /// This class is thread-agnostic and does not provide built-in thread safety. If used in a multithreaded
    /// environment, callers are responsible for synchronizing access to instances as needed. </para></remarks>
    public abstract class AutoNotifiableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore, IgnoredProperty]
        public Dictionary<PropertyInfo, int> PropertyOldHashes { get; } = [];

        [JsonIgnore, IgnoredProperty]
        public Dictionary<PropertyInfo, object> PropertyOldValues { get; } = [];

        [JsonIgnore, IgnoredProperty]
        public bool IsPaused { get; private set; } = false;

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
            NotifyAll();
        }

        public void Notify(string propName, bool postprocess = true)
        {
            Notify(GetType().GetProperty(propName));
        }

        public void NotifyAll(bool postprocess = true)
        {
            foreach (var prop in ListNotifyProperties[GetType()])
            {
                Notify(prop, postprocess);
            }
        }

        private void Notify(PropertyInfo prop, bool postprocess = true)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop.Name));
            if (IsCollectionProperty(prop))
            {
                CollectionViewSource.GetDefaultView(prop.GetValue(this))?.Refresh();
            }
            if (postprocess)
            {
                var oldValue = PropertyOldValues.ContainsKey(prop) ? PropertyOldValues[prop] : 0;
                PostPropertyChanged(prop, oldValue, prop.GetValue(this));
            }
            if (!ListPassiveNotifyProperties[GetType()].Contains(prop))
            {
                NotifyPassives(postprocess, prop);
            }
        }

        private void NotifyPassives(bool postprocess = true, PropertyInfo triggerProp = null)
        {
            foreach (var prop in ListPassiveNotifyProperties[GetType()])
            {
                if (prop == triggerProp)
                    continue;
                OnPropertyChanged(prop, PropertyOldValues.ContainsKey(prop) ? PropertyOldValues[prop] : null, prop.GetValue(this));
            }
        }

        private bool IsCollectionProperty(PropertyInfo prop)
        {
            return prop.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType);
        }

        public void OnPropertyChanged(PropertyInfo prop, object before, object after)
        {
            if (IsPaused)
                return;

            var thisType = prop.DeclaringType;

            if (!ListNotifyProperties.ContainsKey(thisType) ||
                !ListNotifyProperties[thisType].Contains(prop))
                return;

            var oldValueCode = PropertyOldHashes.ContainsKey(prop) ? PropertyOldHashes[prop] : 0;
            var newValueCode = ObjectHelper.GetObjectHashCode(after, null);

            if (!Equals(oldValueCode, newValueCode))
            {
                PropertyOldValues[prop] = after;
                PropertyOldHashes[prop] = newValueCode;

                //notify property changed
                Notify(prop);
            }
        }

        private void PostPropertyChanged(PropertyInfo prop, object before, object after)
        {
            //notify methods
            MethodInfo[] notiMethods = null;
            ListNotifyMethods.TryGetValue(prop, out notiMethods);
            if (notiMethods != null)
            {
                foreach (var m in notiMethods)
                {
                    m.Invoke(this, [this, prop, before, after]);
                }
            }
        }

        public AutoNotifiableObject()
        {
            var thisType = GetType();
            if (!LoadedTypes.Contains(thisType))
            {
                LoadedTypes.Add(thisType);
                PrepareNotifyProperties();
                PrepareNotifyMethods();
            }
        }

        private void PrepareNotifyProperties()
        {
            var thisType = GetType();
            var properties = thisType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ListNotifyProperties[thisType] = properties.Where(p => p.CanRead && p.GetCustomAttribute<IgnoredPropertyAttribute>() == null).ToArray();
            ListPassiveNotifyProperties[thisType] = ListNotifyProperties[thisType].Where(p => !p.CanWrite || IsCollectionProperty(p)).ToArray();
        }

        private void PrepareNotifyMethods()
        {
            var thisType = GetType();

            //class
            var classMethods = new List<MethodInfo>();
            var classMethodAtt = thisType.GetCustomAttribute<NotifyMethodAttribute>();
            if (classMethodAtt != null && classMethodAtt.Methods?.Length > 0)
            {
                foreach (string mName in classMethodAtt.Methods)
                {
                    var runMethod = thisType.GetMethod(mName);
                    CheckNotifyMethod(runMethod);
                    classMethods.Add(runMethod);
                }
            }

            //each properties
            foreach (var p in ListNotifyProperties[thisType])
            {
                var listMethods = new List<MethodInfo>();
                var notiMethodAtt = p.GetCustomAttribute<NotifyMethodAttribute>();
                if (notiMethodAtt != null && notiMethodAtt.Methods?.Length > 0)
                {
                    foreach (string mName in notiMethodAtt.Methods)
                    {
                        var runMethod = thisType.GetMethod(mName);
                        CheckNotifyMethod(runMethod);
                        listMethods.Add(runMethod);
                    }
                }
                listMethods.AddRange(classMethods);
                ListNotifyMethods.Add(p, listMethods.ToArray());
            }
        }

        /// <summary>
        /// Check notifying method
        /// </summary>
        private void CheckNotifyMethod(MethodInfo m)
        {
            if (m == null)
                throw new MissingMethodException();
            if (m.GetParameters().Length != 4 ||
                m.GetParameters()[1].ParameterType != typeof(PropertyInfo))
                throw new ArgumentException();
        }

        [JsonIgnore, IgnoredProperty]
        public static List<Type> LoadedTypes { get; } = [];
        [JsonIgnore, IgnoredProperty]
        public static Dictionary<PropertyInfo, MethodInfo[]> ListNotifyMethods { get; } = [];
        [JsonIgnore, IgnoredProperty]
        public static Dictionary<Type, PropertyInfo[]> ListNotifyProperties { get; } = [];
        [JsonIgnore, IgnoredProperty]
        public static Dictionary<Type, PropertyInfo[]> ListPassiveNotifyProperties { get; } = [];
    }
}