using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using System.Reflection;

namespace ModCreator.WindowData
{
    [SetterAspect]
    public class NewProjectWindowData : CWindowData
    {
        [NotifyMethod(nameof(ValidateInput))]
        public string ProjectName { get; set; }

        [NotifyMethod(nameof(ValidateInput))]
        public string ProjectId { get; set; }

        public string Description { get; set; }
        public string Author { get; set; }
        public bool CanCreate { get; set; }
        public string WorkplacePath { get; set; }
        public ModProject CreatedProject { get; private set; }

        public void ValidateInput(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            CanCreate = !string.IsNullOrWhiteSpace(ProjectName);
            
            // If ProjectId is provided, validate it matches \w+ pattern
            if (!string.IsNullOrWhiteSpace(ProjectId))
            {
                CanCreate = CanCreate && System.Text.RegularExpressions.Regex.IsMatch(ProjectId, @"^\w+$");
            }
        }

        public void CreateProject(string workplacePath)
        {
            // Use provided ProjectId or null to auto-generate
            var projectId = string.IsNullOrWhiteSpace(ProjectId) ? null : ProjectId;
            CreatedProject = ProjectHelper.CreateProject(ProjectName, workplacePath, Description, Author, projectId);
        }
    }
}