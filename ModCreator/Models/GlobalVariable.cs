using ModCreator.Attributes;
using ModCreator.Commons;

namespace ModCreator.Models
{
    /// <summary>
    /// Global variable model
    /// </summary>
    public class GlobalVariable : ValidatedModel
    {
        public string Name { get; set; }
        public string Type { get; set; }

        private string _value;
        [NotifyMethod(nameof(ValidateValueField))]
        public string Value
        {
            get => _value;
            set => _value = value;
        }

        public string Description { get; set; }

        private void ValidateValueField(object obj, System.Reflection.PropertyInfo prop, object oldValue, object newValue)
        {
            if (!ValidateValue(Value, Type))
            {
                SetValidationError($"Invalid value for type {Type}");
            }
            else
            {
                ClearValidation();
            }
        }
    }
}
