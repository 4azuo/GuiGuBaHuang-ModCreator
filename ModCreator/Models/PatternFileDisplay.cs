using ModCreator.Commons;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    public class PatternFileDisplay : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public List<PatternElement> Elements { get; set; } = new List<PatternElement>();
        public List<PatternElement> DisplayElements { get; set; } = new List<PatternElement>();
        public ObservableCollection<RowDisplay> Rows { get; set; } = new ObservableCollection<RowDisplay>();

        public void AddRow()
        {
            var newRow = new Dictionary<string, string>();
            foreach (var element in Elements)
            {
                newRow[element.Name] = element.Value ?? string.Empty;
            }
            Rows.Add(new RowDisplay(newRow, Elements, DisplayElements));
        }

        public void RemoveRow(RowDisplay row)
        {
            Rows.Remove(row);
        }
    }
}
