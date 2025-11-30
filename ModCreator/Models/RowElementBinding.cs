using ModCreator.Commons;
using ModCreator.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.Models
{
    public class RowElementBinding : AutoNotifiableObject
    {
        private Dictionary<string, string> _row;
        private List<PatternElement> _allElements;
        private ObservableCollection<RowElementBinding> _allBindings;

        public PatternElement Element { get; set; }

        public string Value
        {
            get => _row.ContainsKey(Element.Name) ? _row[Element.Name] : string.Empty;
            set
            {
                var oldValue = Value;
                if (_row.ContainsKey(Element.Name))
                    _row[Element.Name] = value;
                else
                    _row.Add(Element.Name, value);

                if (oldValue != value)
                {
                    Notify(this, nameof(Value));
                    ProcessAutoGeneration();
                }
            }
        }

        public RowElementBinding(Dictionary<string, string> row, PatternElement element, List<PatternElement> allElements = null, ObservableCollection<RowElementBinding> allBindings = null)
        {
            _row = row;
            _allElements = allElements;
            _allBindings = allBindings;
            Element = element;
        }

        private void ProcessAutoGeneration()
        {
            if (_allElements == null || _allBindings == null)
                return;

            foreach (var element in _allElements.Where(e => !string.IsNullOrEmpty(e.AutoGenPattern)))
            {
                var generatedValue = PatternHelper.ProcessAutoGenValue(element.AutoGenPattern, _row);
                if (!string.IsNullOrEmpty(generatedValue))
                {
                    if (_row.ContainsKey(element.Name))
                        _row[element.Name] = generatedValue;
                    else
                        _row.Add(element.Name, generatedValue);
                    
                    var targetBinding = _allBindings.FirstOrDefault(b => b.Element.Name == element.Name);
                    if (targetBinding != null)
                    {
                        targetBinding.Notify(targetBinding, nameof(Value));
                    }
                }
            }
        }
    }
}
