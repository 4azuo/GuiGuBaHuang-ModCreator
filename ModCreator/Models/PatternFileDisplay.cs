using ModCreator.Commons;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    public class PatternFileDisplay : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public List<PatternElement> Elements { get; set; }
        public ObservableCollection<RowDisplay> Rows { get; set; }

        public PatternFileDisplay()
        {
            Elements = new List<PatternElement>();
            Rows = new ObservableCollection<RowDisplay>();
        }

        public void AddRow()
        {
            var newRow = new Dictionary<string, string>();
            foreach (var element in Elements)
            {
                newRow[element.Name] = element.Value ?? string.Empty;
            }
            Rows.Add(new RowDisplay(newRow, Elements));
        }

        public void RemoveRow(RowDisplay row)
        {
            Rows.Remove(row);
        }
    }
}
