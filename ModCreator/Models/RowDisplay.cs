using ModCreator.Attributes;
using ModCreator.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ModCreator.Models
{
    [SetterAspect]
    public class RowDisplay : AutoNotifiableObject
    {
        public Dictionary<string, string> RowData { get; set; }
        public List<RowElementBinding> Bindings { get; set; } = new List<RowElementBinding>();
        public int FrozenColumns { get; set; }

        public RowDisplay(Dictionary<string, string> rowData, List<PatternElement> allElements, List<PatternElement> displayElements = null, int frozenColumns = 2)
        {
            RowData = rowData;
            FrozenColumns = frozenColumns;
            var elementsToDisplay = displayElements ?? allElements;
            
            foreach (var element in elementsToDisplay)
            {
                Bindings.Add(new RowElementBinding(rowData, element, allElements, Bindings));
            }
        }
        
        public List<RowElementBinding> FrozenBindings => [.. Bindings.Take(Math.Min(FrozenColumns, Bindings.Count))];
        
        public List<RowElementBinding> ScrollableBindings => [.. Bindings.Skip(Math.Min(FrozenColumns, Bindings.Count))];
    }
}
