using ModCreator.Commons;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.Models
{
    public class RowDisplay : AutoNotifiableObject
    {
        public Dictionary<string, string> RowData { get; set; }
        public ObservableCollection<RowElementBinding> Bindings { get; set; }

        public RowDisplay(Dictionary<string, string> rowData, List<PatternElement> allElements, List<PatternElement> displayElements = null)
        {
            RowData = rowData;
            var elementsToDisplay = displayElements ?? allElements;
            Bindings = new ObservableCollection<RowElementBinding>();
            
            foreach (var element in elementsToDisplay)
            {
                Bindings.Add(new RowElementBinding(rowData, element, allElements, Bindings));
            }
        }
    }
}
