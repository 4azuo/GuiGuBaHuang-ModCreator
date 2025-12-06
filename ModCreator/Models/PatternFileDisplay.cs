using ModCreator.Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.Models
{
    public class PatternFileDisplay : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public List<PatternElement> Elements { get; set; } = new List<PatternElement>();
        public List<PatternElement> DisplayElements { get; set; } = new List<PatternElement>();
        public ObservableCollection<RowDisplay> Rows { get; set; } = new ObservableCollection<RowDisplay>();
        public int FrozenColumns { get; set; } = 2;
        
        public List<PatternElement> FrozenDisplayElements => 
            DisplayElements.Take(Math.Min(FrozenColumns, DisplayElements.Count)).ToList();
        
        public List<PatternElement> ScrollableDisplayElements => 
            DisplayElements.Skip(Math.Min(FrozenColumns, DisplayElements.Count)).ToList();

        public void AddRow()
        {
            var newRow = new Dictionary<string, string>();
            foreach (var element in Elements)
            {
                newRow[element.Name] = element.Value ?? string.Empty;
            }
            Rows.Add(new RowDisplay(newRow, Elements, DisplayElements, FrozenColumns));
        }

        public void RemoveRow(RowDisplay row)
        {
            Rows.Remove(row);
        }
    }
}
