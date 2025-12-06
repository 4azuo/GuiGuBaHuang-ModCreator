using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Linq;

namespace ModCreator.Commons
{
    /// <summary>
    /// var-types.json driven model with validation capabilities
    /// </summary>
    public abstract class ValidatedModel : AutoNotifiableObject
    {
        public static List<VarType> VarTypes { get; private set; }

        static ValidatedModel()
        {
            LoadVarTypes();
        }

        private static void LoadVarTypes()
        {
            VarTypes = ResourceHelper.ReadEmbeddedResource<List<VarType>>("ModCreator.Resources.var-types.json");
        }

        public string ValidationError { get; set; }

        public bool IsValid => string.IsNullOrEmpty(ValidationError);

        public bool ValidateValue(string value, string varType)
        {
            if (string.IsNullOrEmpty(varType))
                return true;

            var typeInfo = VarTypes.FirstOrDefault(t => t.Type == varType);
            if (typeInfo == null)
                return true;

            if (string.IsNullOrWhiteSpace(value))
                return true;

            switch (varType)
            {
                case "Boolean":
                    if (!bool.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid boolean value: {value}";
                        return false;
                    }
                    break;

                case "Int32":
                    if (!int.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid integer value: {value}";
                        return false;
                    }
                    break;

                case "Int64":
                    if (!long.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid long value: {value}";
                        return false;
                    }
                    break;

                case "Single":
                    if (!float.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid float value: {value}";
                        return false;
                    }
                    break;

                case "Double":
                    if (!double.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid double value: {value}";
                        return false;
                    }
                    break;

                case "String":
                    break;

                case "Array":
                    break;

                default:
                    break;
            }

            ValidationError = null;
            return true;
        }

        public void ClearValidation()
        {
            ValidationError = null;
            Notify(nameof(ValidationError));
            Notify(nameof(IsValid));
        }

        public void SetValidationError(string error)
        {
            ValidationError = error;
            Notify(nameof(ValidationError));
            Notify(nameof(IsValid));
        }
    }
}
