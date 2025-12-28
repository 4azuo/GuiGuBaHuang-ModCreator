using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    [SetterAspect]
    public partial class ProjectEditorWindowData : CWindowData
    {
        private ModProject _originalProject;
        private string _statusMessage;

        [NotifyMethod(nameof(LoadProjectData))]
        public ModProject Project { get; set; }

        // Language properties for translation
        public List<Language> SourceLanguages => ModConfHelper.LoadLanguages();
        public Language SelectedSourceLanguage { get; set; }

        public string StatusMessage
        {
            get => _statusMessage;
            set => _statusMessage = $"{DateTime.Now:HH:mm:ss} - {value}";
        }

        public bool HasUnsavedChanges()
        {
            if (Project == null || _originalProject == null) return false;

            // Only check ModProject properties, not nested collections
            return !Helpers.ObjectHelper.ArePropertiesEqual(Project, _originalProject, [
                typeof(ModProject),
                typeof(GlobalVariable),
                typeof(FileItem),
                typeof(ModEventItem),
                typeof(EventActionBase),
                typeof(Models.ParameterInfo),
                typeof(ModEventItemSelectValue),
            ]);
        }

        public void BackupProject()
        {
            if (Project == null) return;
            _originalProject = Project.Clone();
        }

        public void RestoreProject()
        {
            if (_originalProject == null) return;
            Project = _originalProject.Clone();
            SaveProject();
        }

        public void LoadProjectData(object obj, PropertyInfo prop, object before = null, object after = null)
        {
            if (Project == null) return;

            LoadConfFiles();
            LoadCustomResourceFiles();
            LoadGlobalVariables();
            LoadModEventFiles();

            BackupProject();
            
            SelectedSourceLanguage = SourceLanguages.FirstOrDefault();
            StatusMessage = MessageHelper.GetFormat("Messages.Success.LoadedProjects", Project.ProjectName);
        }

        public void ReloadProjectData()
        {
            LoadProjectData(this, null);
        }

        public void SaveProject()
        {
            if (Project == null) return;

            SaveConfContent();
            
            // Save global variables from container back to Project
            if (GlobalVariablesContainer != null)
                Project.GlobalVariables = GlobalVariablesContainer.Variables;
            
            SaveModEvents();

            Project.LastModifiedDate = DateTime.Now;
            
            // Save current project to its project.json file
            ProjectHelper.SaveProject(Project);

            BackupProject(); // Update backup after successful save
        }
    }
}