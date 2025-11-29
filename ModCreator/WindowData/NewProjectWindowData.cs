using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Reflection;

namespace ModCreator.WindowData
{
    public class NewProjectWindowData : CWindowData
    {
        [NotifyMethod(nameof(ValidateInput))]
        public string ProjectName { get; set; }

        public string Description { get; set; }
        public string Author { get; set; }
        public bool CanCreate { get; set; }
        public string WorkplacePath { get; set; }
        public ModProject CreatedProject { get; private set; }

        public void ValidateInput(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            CanCreate = !string.IsNullOrWhiteSpace(ProjectName);
        }

        public void CreateProject(string workplacePath)
        {
            CreatedProject = ProjectHelper.CreateProject(ProjectName, workplacePath, Description, Author);
        }
    }
}