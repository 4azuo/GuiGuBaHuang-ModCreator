using ModCreator.Commons;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents an action in ModEvent
    /// </summary>
    public class EventAction : AutoNotifiableObject
    {
        /// <summary>
        /// Action name/type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name for UI
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description of the action
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// C# code for the action
        /// </summary>
        public string Code { get; set; }
    }
}
