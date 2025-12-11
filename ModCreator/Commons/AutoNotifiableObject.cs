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
    /// Provides a base class that implements automatic property change notification and periodic update functionality for derived objects.
    /// Important: Requires SetterAspectAttribute on property setters or class to function correctly.
    /// </summary>
    /// <remarks><para> <b>AutoNotifiableObject</b> enables automatic notification of property changes by
    /// implementing <see cref="INotifyPropertyChanged"/>. It also supports periodic updates via a dispatcher timer,
    /// allowing properties to be refreshed and notifications to be sent at regular intervals. </para> <para> The class
    /// manages notification state, including pausing and resuming notifications, and tracks old property values to
    /// prevent redundant change events. It is designed for use in scenarios where property changes need to be observed,
    /// such as data binding in UI frameworks. </para> <para> Derived classes can leverage the built-in mechanisms for
    /// property change notification and timer-based updates without manually implementing <see
    /// cref="INotifyPropertyChanged"/> logic. </para> <para> <b>Thread Safety:</b> This class is not thread-safe. All
    /// interactions should occur on the UI thread associated with the dispatcher. </para> <para> <b>Disposal:</b> When
    /// disposed, the periodic update timer is stopped and event handlers are detached to release resources.
    /// </para></remarks>
    public abstract class AutoNotifiableObject : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore, IgnoredProperty]
        public DispatcherTimer AutoUpdateTimer { get; } = new(TimeSpan.FromMilliseconds(AUTO_RENOTIFY_PERIOD), DispatcherPriority.Background, (s, e) => { }, Application.Current.Dispatcher);

        [JsonIgnore, IgnoredProperty]
        public Dictionary<PropertyInfo, int> PropertyOldValues { get; } = [];

        [JsonIgnore, IgnoredProperty]
        public bool IsPaused { get; private set; } = false;

        [JsonIgnore, IgnoredProperty]
        public bool IsConstructing { get; private set; } = true;

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
                PostPropertyChanged(prop, null, prop.GetValue(this));
            }
        }

        private bool IsCollectionProperty(PropertyInfo prop)
        {
            return prop.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType);
        }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            if (IsPaused)
                return;

            if (IsConstructing)
                return;

            var thisType = GetType();
            var prop = thisType.GetProperty(propertyName);

            if (!ListNotifyProperties.ContainsKey(thisType) ||
                !ListNotifyProperties[thisType].Contains(prop))
                return;

            var oldValueCode = PropertyOldValues.ContainsKey(prop) ? PropertyOldValues[prop] : 0;
            var newValueCode = ObjectHelper.GetObjectHashCode(after, null);

            if (!Equals(oldValueCode, newValueCode))
            {
                PropertyOldValues[prop] = newValueCode;

                //notify property changed
                Notify(prop, false);
                PostPropertyChanged(prop, before, after);
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

        private void AutoUpdate(object sender, EventArgs e)
        {
            if (IsPaused)
                return;
            if (IsConstructing)
            {
                IsConstructing = false;
                Resume();
            }
            else
            {
                PropertyInfo[] properties;
                if (!ListAutoNotifyProperties.TryGetValue(GetType(), out properties))
                    return;

                foreach (var item in properties)
                {
                    OnPropertyChanged(item.Name, null, item.GetValue(this));
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
            AutoUpdateTimer.Tick += AutoUpdate;
            AutoUpdateTimer.Start();
        }

        public void Dispose()
        {
            AutoUpdateTimer.Tick -= AutoUpdate;
            AutoUpdateTimer.Stop();
        }

        private void PrepareNotifyProperties()
        {
            var thisType = GetType();
            var properties = thisType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ListNotifyProperties[thisType] = properties.Where(p => p.CanRead && p.GetCustomAttribute<IgnoredPropertyAttribute>() == null).ToArray();
            ListAutoNotifyProperties[thisType] = ListNotifyProperties[thisType].Where(p => !p.CanWrite || IsCollectionProperty(p)).ToArray();
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

        public const int AUTO_RENOTIFY_PERIOD = 200;

        [JsonIgnore, IgnoredProperty]
        public static List<Type> LoadedTypes { get; } = [];
        [JsonIgnore, IgnoredProperty]
        public static Dictionary<PropertyInfo, MethodInfo[]> ListNotifyMethods { get; } = [];
        [JsonIgnore, IgnoredProperty]
        public static Dictionary<Type, PropertyInfo[]> ListNotifyProperties { get; } = [];
        [JsonIgnore, IgnoredProperty]
        public static Dictionary<Type, PropertyInfo[]> ListAutoNotifyProperties { get; } = [];
    }
}