using ModCreator.Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.Models
{
    public class RowDisplay : AutoNotifiableObject
    {
        public Dictionary<string, string> RowData { get; set; }
        public ObservableCollection<RowElementBinding> Bindings { get; set; } = new ObservableCollection<RowElementBinding>();
        private int _frozenColumns;

        public RowDisplay(Dictionary<string, string> rowData, List<PatternElement> allElements, List<PatternElement> displayElements = null, int frozenColumns = 2)
        {
            RowData = rowData;
            _frozenColumns = frozenColumns;
            var elementsToDisplay = displayElements ?? allElements;
            
            foreach (var element in elementsToDisplay)
            {
                Bindings.Add(new RowElementBinding(rowData, element, allElements, Bindings));
            }
        }
        
        public ObservableCollection<RowElementBinding> FrozenBindings => 
            new ObservableCollection<RowElementBinding>(Bindings.Take(Math.Min(_frozenColumns, Bindings.Count)));
        
        public ObservableCollection<RowElementBinding> ScrollableBindings => 
            new ObservableCollection<RowElementBinding>(Bindings.Skip(Math.Min(_frozenColumns, Bindings.Count)));
    }
}
