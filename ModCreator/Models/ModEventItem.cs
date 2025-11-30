using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModCreator.Commons;
using ModCreator.Enums;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents a ModEvent with its configuration
    /// </summary>
    public class ModEventItem : AutoNotifiableObject
    {
        /// <summary>
        /// Event mode: ModEvent or NonEvent
        /// </summary>
        public EventMode EventMode { get; set; } = EventMode.ModEvent;

        /// <summary>
        /// Custom event method name (for NonEvent mode)
        /// </summary>
        public string CustomEventName { get; set; }

        /// <summary>
        /// Cache type for the event (e.g., Local, Global)
        /// </summary>
        public string CacheType { get; set; }

        /// <summary>
        /// WorkOn type (e.g., All, Local, Global)
        /// </summary>
        public string WorkOn { get; set; }

        /// <summary>
        /// Order index for event execution
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Selected event method name
        /// </summary>
        public string SelectedEvent { get; set; }

        /// <summary>
        /// Logic operator for combining conditions: "AND" or "OR"
        /// </summary>
        public string ConditionLogic { get; set; } = "AND";

        /// <summary>
        /// List of conditions for this event
        /// </summary>
        public ObservableCollection<EventCondition> Conditions { get; set; } = [];

        /// <summary>
        /// List of actions for this event
        /// </summary>
        public ObservableCollection<EventAction> Actions { get; set; } = [];

        /// <summary>
        /// Full file path to the ModEvent .cs file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// File name without path (without extension, used as class name)
        /// </summary>
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(FilePath);

        /// <summary>
        /// Flag indicating if this event is in code-only mode (cannot switch back to GUI mode)
        /// </summary>
        public bool IsCodeModeOnly { get; set; }
    }
}
