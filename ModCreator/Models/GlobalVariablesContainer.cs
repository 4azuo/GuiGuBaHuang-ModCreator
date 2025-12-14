using ModCreator.Attributes;
using ModCreator.Commons;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    /// <summary>
    /// Container for global variables with history tracking (Undo/Redo support)
    /// </summary>
    [SetterAspect]
    public class GlobalVariablesContainer : HistorableObject
    {
        /// <summary>
        /// Collection of global variables
        /// </summary>
        public ObservableCollection<GlobalVariable> Variables { get; set; } = [];
    }
}
