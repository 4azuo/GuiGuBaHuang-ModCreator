using ModCreator.Attributes;
using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class PatternElement : ValidatedModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string VarType { get; set; }
        public bool Enable { get; set; }
        public bool Required { get; set; }
        public List<ModConfValue> Options { get; set; }
        [NotifyMethod(nameof(ValidateValueField))]
        public string Value { get; set; }

        public PatternElement()
        {
            Options = new List<ModConfValue>();
            Enable = true;
            Required = false;
        }

        private void ValidateValueField(object obj, System.Reflection.PropertyInfo prop, object oldValue, object newValue)
        {
            if (!ValidateValue(Value, VarType))
            {
                SetValidationError($"Invalid value for type {VarType}");
            }
            else
            {
                ClearValidation();
            }
        }
    }
}
