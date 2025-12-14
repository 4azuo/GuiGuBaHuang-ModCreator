using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Linq;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public List<VarType> VarTypes { get; set; } = ValidatedModel.VarTypes;
        public GlobalVariablesContainer GlobalVariablesContainer { get; set; }
        
        public bool CanUndoVariables => GlobalVariablesContainer?.CanUndo ?? false;
        public bool CanRedoVariables => GlobalVariablesContainer?.CanRedo ?? false;
        
        public bool IsVariablesGridViewVisible => !IsVariablesSourceViewVisible;
        public bool IsVariablesSourceViewVisible { get; set; } = false;
        
        private bool _variablesSortAscending = true;

        public void LoadGlobalVariables()
        {
            // Create temporary container from Project.GlobalVariables
            GlobalVariablesContainer = new GlobalVariablesContainer();
            GlobalVariablesContainer.Variables.ReplaceWith(Project.GlobalVariables);
        }
        
        public void UndoVariables()
        {
            if (GlobalVariablesContainer != null && GlobalVariablesContainer.CanUndo)
            {
                GlobalVariablesContainer.Undo();
                StatusMessage = MessageHelper.Get("Messages.Success.Undo");
            }
        }
        
        public void RedoVariables()
        {
            if (GlobalVariablesContainer != null && GlobalVariablesContainer.CanRedo)
            {
                GlobalVariablesContainer.Redo();
                StatusMessage = MessageHelper.Get("Messages.Success.Redo");
            }
        }
        
        public void ToggleToGridView()
        {
            IsVariablesSourceViewVisible = false;
            StatusMessage = MessageHelper.Get("Messages.Success.SwitchedToGridView");
        }
        
        public void ToggleToSourceView()
        {
            IsVariablesSourceViewVisible = true;
            StatusMessage = MessageHelper.Get("Messages.Success.SwitchedToSourceView");
        }
        
        public void SortVariablesByName()
        {
            if (GlobalVariablesContainer?.Variables == null || GlobalVariablesContainer.Variables.Count == 0)
                return;
                
            var sorted = _variablesSortAscending
                ? GlobalVariablesContainer.Variables.OrderBy(v => v.Name).ToList()
                : GlobalVariablesContainer.Variables.OrderByDescending(v => v.Name).ToList();
            
            GlobalVariablesContainer.Variables.Clear();
            foreach (var variable in sorted)
            {
                GlobalVariablesContainer.Variables.Add(variable);
            }
            
            _variablesSortAscending = !_variablesSortAscending;
            StatusMessage = MessageHelper.GetFormat("Messages.Success.SortedVariables", _variablesSortAscending ? "descending" : "ascending");
        }
    }
}
