using ModCreator.Commons;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents a condition in ModEvent
    /// </summary>
    public class EventCondition : AutoNotifiableObject
    {
        /// <summary>
        /// Condition name/type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name for UI
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description of the condition
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// C# code for the condition
        /// </summary>
        public string Code { get; set; }
    }
}
