using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        public ObservableCollection<GlobalVariable> GlobalVariables { get; set; } = new ObservableCollection<GlobalVariable>();
        public List<VarType> VarTypes { get; set; } = ResourceHelper.ReadEmbeddedResource<List<VarType>>("ModCreator.Resources.var-types.json");

        public void LoadGlobalVariables()
        {
            GlobalVariables = new ObservableCollection<GlobalVariable>(Project.GlobalVariables);
        }

        public void SaveGlobalVariables()
        {
            if (Project == null) return;

            Project.GlobalVariables = new List<GlobalVariable>(GlobalVariables);
        }
    }
}