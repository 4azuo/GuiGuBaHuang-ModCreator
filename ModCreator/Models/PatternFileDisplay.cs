using ModCreator.Commons;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    public class PatternFileDisplay : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public List<PatternElement> Elements { get; set; }
        public ObservableCollection<Dictionary<string, string>> Rows { get; set; }

        public PatternFileDisplay()
        {
            Elements = new List<PatternElement>();
            Rows = new ObservableCollection<Dictionary<string, string>>();
        }

        public void AddRow()
        {
            var newRow = new Dictionary<string, string>();
            foreach (var element in Elements)
            {
                newRow[element.Name] = string.Empty;
            }
            Rows.Add(newRow);
        }

        public void RemoveRow(Dictionary<string, string> row)
        {
            Rows.Remove(row);
        }
    }
}
