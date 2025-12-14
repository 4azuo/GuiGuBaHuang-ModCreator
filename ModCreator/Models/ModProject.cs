using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Enums;
using System;
using System.Collections.ObjectModel;

namespace ModCreator.Models
{
    /// <summary>
    /// Represents a mod project
    /// </summary>
    [SetterAspect]
    public class ModProject : AutoNotifiableObject
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string TitleImg { get; set; }
        public ProjectState State { get; set; } = ProjectState.Valid;
        public bool AutoSaveEnabled { get; set; } = false;
        public ObservableCollection<GlobalVariable> GlobalVariables { get; set; } = [];
        public ObservableCollection<ModEventItem> ModEvents { get; set; } = [];
    }
}
