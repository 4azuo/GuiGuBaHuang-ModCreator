using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Linq;

namespace ModCreator.Commons
{
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
                case "bool":
                    if (!bool.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid boolean value: {value}";
                        return false;
                    }
                    break;

                case "int":
                    if (!int.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid integer value: {value}";
                        return false;
                    }
                    break;

                case "long":
                    if (!long.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid long value: {value}";
                        return false;
                    }
                    break;

                case "float":
                    if (!float.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid float value: {value}";
                        return false;
                    }
                    break;

                case "double":
                    if (!double.TryParse(value, out _))
                    {
                        ValidationError = $"Invalid double value: {value}";
                        return false;
                    }
                    break;

                case "string":
                    break;

                default:
                    if (varType.EndsWith("[]"))
                    {
                        var baseType = varType.Substring(0, varType.Length - 2);
                        var values = value.Split(',');
                        foreach (var val in values)
                        {
                            if (!ValidateValue(val.Trim(), baseType))
                                return false;
                        }
                    }
                    break;
            }

            ValidationError = null;
            return true;
        }

        public void ClearValidation()
        {
            ValidationError = null;
            Notify(this, nameof(ValidationError));
            Notify(this, nameof(IsValid));
        }

        public void SetValidationError(string error)
        {
            ValidationError = error;
            Notify(this, nameof(ValidationError));
            Notify(this, nameof(IsValid));
        }
    }
}
