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
    public partial class ProjectEditorWindowData : CWindowData
    {
        private ModProject _originalProject;
        private string _statusMessage;

        [NotifyMethod(nameof(LoadProjectData))]
        public ModProject Project { get; set; }

        // Language properties for translation
        public List<Language> SourceLanguages { get; set; } = [];
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

        public void LoadProjectData(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (Project == null) return;

            LoadConfFiles();
            LoadImageFiles();
            LoadGlobalVariables();
            LoadModEventFiles();
            LoadModResources();

            BackupProject();
            StatusMessage = MessageHelper.GetFormat("Messages.Success.LoadedProjects", Project.ProjectName);
        }

        public void ReloadProjectData()
        {
            LoadProjectData(this, null, null, null);
        }

        public void SaveProject()
        {
            if (Project == null) return;

            SaveConfContent();
            SaveGlobalVariables();
            SaveModEvents();

            Project.LastModifiedDate = DateTime.Now;
            
            // Save current project to its project.json file
            ProjectHelper.SaveProject(Project);
            BackupProject(); // Update backup after successful save
        }

        public void LoadModResources()
        {
            var resourcePrefix = "ModCreator.Resources.";

            SourceLanguages = ResourceHelper.ReadEmbeddedResource<List<Language>>($"{resourcePrefix}languages.json");
            CacheTypes = ResourceHelper.ReadEmbeddedResource<List<string>>($"{resourcePrefix}modevent-cachetype.json");
            WorkOnTypes = ResourceHelper.ReadEmbeddedResource<List<string>>($"{resourcePrefix}modevent-workon.json");

            SelectedSourceLanguage = SourceLanguages[0];
        }
    }
}