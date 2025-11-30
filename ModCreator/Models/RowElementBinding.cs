using ModCreator.Commons;
using System.Collections.Generic;

namespace ModCreator.Models
{
    public class RowElementBinding : AutoNotifiableObject
    {
        private Dictionary<string, string> _row;
        private PatternElement _element;

        public PatternElement Element
        {
            get => _element;
            set => _element = value;
        }

        public string Value
        {
            get => _row.ContainsKey(_element.Name) ? _row[_element.Name] : string.Empty;
            set
            {
                if (_row.ContainsKey(_element.Name))
                    _row[_element.Name] = value;
                else
                    _row.Add(_element.Name, value);
            }
        }

        public RowElementBinding(Dictionary<string, string> row, PatternElement element)
        {
            _row = row;
            _element = element;
        }
    }
}
