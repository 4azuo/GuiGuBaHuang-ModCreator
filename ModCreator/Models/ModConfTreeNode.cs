using ModCreator.Commons;
using ModCreator.Enums;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    public class ModConfTreeNode : AutoNotifiableObject
    {
        public string DisplayName { get; set; }
        public ModConfNodeType Type { get; set; }
        public string Value { get; set; }
        public string FilePath { get; set; }
        public string FieldName { get; set; }
        public ObservableCollection<ModConfTreeNode> Children { get; set; } = [];
    }
}
