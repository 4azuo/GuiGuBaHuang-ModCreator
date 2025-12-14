using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.Models
{
    [SetterAspect]
    public class RowDisplay : AutoNotifiableObject
    {
        public Dictionary<string, string> RowData { get; set; }
        public int FrozenColumns { get; set; }
        public ObservableCollection<RowElementBinding> Bindings { get; set; } = [];
        public ObservableCollection<RowElementBinding> FrozenBindings =>
            Bindings.Take(Math.Min(FrozenColumns, Bindings.Count)).ToOC();
        public ObservableCollection<RowElementBinding> ScrollableBindings =>
            Bindings.Skip(Math.Min(FrozenColumns, Bindings.Count)).ToOC();

        public RowDisplay(Dictionary<string, string> rowData, ObservableCollection<PatternElement> allElements, ObservableCollection<PatternElement> displayElements, int frozenColumns)
        {
            RowData = rowData;
            FrozenColumns = frozenColumns;
            
            var elementsToDisplay = displayElements ?? allElements;
            
            foreach (var element in elementsToDisplay)
            {
                Bindings.Add(new RowElementBinding(rowData, element, allElements, Bindings));
            }
        }
    }
}
