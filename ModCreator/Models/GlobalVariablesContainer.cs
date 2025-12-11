using ModCreator.Attributes;
using ModCreator.Commons;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

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
        public List<GlobalVariable> Variables { get; set; } = [];
    }
}
