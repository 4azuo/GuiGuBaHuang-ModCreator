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
    public class PatternFileDisplay : AutoNotifiableObject
    {
        public string FileName { get; set; }
        public int FrozenColumns { get; set; }
        public ObservableCollection<PatternElement> Elements { get; set; } = [];
        public ObservableCollection<PatternElement> DisplayElements { get; set; } = [];
        public ObservableCollection<RowDisplay> Rows { get; set; } = [];
        public ObservableCollection<PatternElement> FrozenDisplayElements => 
            DisplayElements.Take(Math.Min(FrozenColumns, DisplayElements.Count)).ToOC();
        public ObservableCollection<PatternElement> ScrollableDisplayElements => 
            DisplayElements.Skip(Math.Min(FrozenColumns, DisplayElements.Count)).ToOC();

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
