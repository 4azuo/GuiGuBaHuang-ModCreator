using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public ObservableCollection<GlobalVariable> GlobalVariables { get; set; } = [];
        public List<VarType> VarTypes { get; set; } = ValidatedModel.VarTypes;

        public void LoadGlobalVariables()
        {
            GlobalVariables = new ObservableCollection<GlobalVariable>(Project.GlobalVariables);
        }

        public void SaveGlobalVariables()
        {
            if (Project == null) return;

            Project.GlobalVariables = new List<GlobalVariable>(GlobalVariables);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.SavedGlobalVariables", GlobalVariables.Count);
        }
    }
}