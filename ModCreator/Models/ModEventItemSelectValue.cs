using ModCreator.Commons;
using ModCreator.Enums;
using ModCreator.Helpers;

namespace ModCreator.Models
{
    public class ModEventItemSelectValue : AutoNotifiableObject
    {

        public EventActionBase SelectedEventAction { get; private set; }
        public GlobalVariable SelectedVariable { get; private set; }
        public string OptionalValue { get; private set; }
        public ModEventSelectType SelectType { get; private set; }

        public ModEventItemSelectValue(EventActionBase eventAction)
        {
            SelectedEventAction = eventAction;
            SelectType = ModEventSelectType.EventAction;
        }

        public ModEventItemSelectValue(GlobalVariable variable)
        {
            SelectedVariable = variable;
            SelectType = ModEventSelectType.Variable;
        }

        public ModEventItemSelectValue(string optionalValue, string returnType)
        {
            OptionalValue = optionalValue;
            SelectType = ModEventSelectType.OptionalValue;
        }

        public string Category
        {
            get => SelectType == ModEventSelectType.EventAction ? SelectedEventAction?.Category : SelectedVariable?.Name;
            set
            {
                if (SelectType == ModEventSelectType.EventAction && SelectedEventAction != null)
                    SelectedEventAction.Category = value;
            }
        }

        public string Name
        {
            get => SelectType == ModEventSelectType.EventAction ? SelectedEventAction?.Name : 
                   SelectType == ModEventSelectType.Variable ? SelectedVariable?.Name : 
                   OptionalValue;
            set
            {
                if (SelectType == ModEventSelectType.EventAction && SelectedEventAction != null)
                    SelectedEventAction.Name = value;
                else if (SelectType == ModEventSelectType.Variable && SelectedVariable != null)
                    SelectedVariable.Name = value;
            }
        }

        public string DisplayName
        {
            get => SelectType == ModEventSelectType.EventAction ? DisplayNameHelper.BuildNestedDisplayName(this) : 
                   SelectType == ModEventSelectType.Variable ? SelectedVariable?.Name : 
                   OptionalValue;
            set
            {
                if (SelectType == ModEventSelectType.EventAction && SelectedEventAction != null)
                    SelectedEventAction.DisplayName = value;
            }
        }

        public string Description
        {
            get => SelectType == ModEventSelectType.EventAction ? SelectedEventAction?.Description : SelectedVariable?.Description;
            set
            {
                if (SelectType == ModEventSelectType.EventAction && SelectedEventAction != null)
                    SelectedEventAction.Description = value;
                else if (SelectType == ModEventSelectType.Variable && SelectedVariable != null)
                    SelectedVariable.Description = value;
            }
        }

        public string Code
        {
            get => SelectType == ModEventSelectType.EventAction ? SelectedEventAction?.Code : 
                   SelectType == ModEventSelectType.Variable ? SelectedVariable?.Name : 
                   OptionalValue;
            set
            {
                if (SelectType == ModEventSelectType.EventAction && SelectedEventAction != null)
                    SelectedEventAction.Code = value;
            }
        }
    }
}