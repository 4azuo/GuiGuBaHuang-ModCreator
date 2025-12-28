using MethodDecorator.Fody.Interfaces;
using ModCreator.Commons;
using ModCreator.Helpers;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace ModCreator.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SetterAspectAttribute : Attribute, IMethodDecorator
    {
        private AutoNotifiableObject _instance;
        //private MethodBase _method;
        private object[] _args;

        private object _oldValue;
        private object _newValue;
        private string _propertyName;
        private PropertyInfo _property;

        public void Init(object instance, MethodBase method, object[] args)
        {
            if (instance is not AutoNotifiableObject obj || method == null)
                return;

            _instance = obj;
            //_method = method;
            _args = args;

            var name = method.Name;
            if (!name.StartsWith("set_", StringComparison.Ordinal))
                return;

            _propertyName = name.Substring(4);

            // Lấy PropertyInfo từ cache hoặc tạo mới
            var typeCache = AutoNotifiableObject.ListNotifyProperties[_instance.GetType()];
            _property = typeCache.FirstOrDefault(p => p.Name == _propertyName);

            if (_property != null && _property.CanRead)
            {
                try { _oldValue = _property.GetValue(_instance); }
                catch { _oldValue = null; }
            }
        }

        public void OnEntry()
        {
            if (_instance == null || _property == null) return;
            _newValue = (_args != null && _args.Length > 0) ? _args[0] : null;
        }

        public void OnExit()
        {
            if (_instance == null || _property == null) return;
            if (AutoNotifiableObject.ListPassiveNotifyProperties[_instance.GetType()].Contains(_property))
            {
                _instance.OnPropertyChanged(_property, _oldValue, _newValue);
            }
            else
            {
                if (_instance.UpdatingProperties.ContainsKey(_property))
                {
                    var lastUpdate = _instance.UpdatingProperties[_property];
                    _instance.UpdatingProperties[_property] = new { lastUpdate.OldValue, NewValue = _newValue, Timestamp = DateTime.Now };
                }
                else
                {
                    _instance.UpdatingProperties[_property] = new { OldValue = _oldValue, NewValue = _newValue, Timestamp = DateTime.Now };
                }
            }
        }

        public void OnException(Exception exception)
        {
            if (_instance != null && _property != null)
                DebugHelper.ShowError($"[SetterAspect] Exception in {_propertyName}: {exception.Message}");
            else
                throw exception;
        }
    }
}