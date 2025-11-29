using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        internal ModProject _originalProject;

        [NotifyMethod(nameof(LoadProjectData))]
        public ModProject Project { get; set; }

        public bool HasUnsavedChanges()
        {
            if (Project == null || _originalProject == null) return false;
            return !ObjectHelper.ArePropertiesEqual(Project, _originalProject);
        }

        public void BackupProject()
        {
            if (Project == null) return;

            var json = JsonConvert.SerializeObject(Project);
            _originalProject = JsonConvert.DeserializeObject<ModProject>(json);
        }

        public void RestoreProject()
        {
            if (_originalProject == null || Project == null) return;

            ObjectHelper.CopyProperties(_originalProject, Project);
            LoadGlobalVariables();
        }

        public void LoadProjectData(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (Project == null) return;

            LoadConfFiles();
            LoadImageFiles();
            LoadGlobalVariables();
            LoadModEventFiles();
            LoadModEventResources();

            BackupProject();
        }

        public void ReloadProjectData()
        {
            LoadProjectData(this, null, null, null);
        }

        public void SaveProject()
        {
            if (Project == null) return;

            if (HasSelectedConfFile && !string.IsNullOrEmpty(SelectedConfContent))
                SaveConfContent();

            SaveGlobalVariables();

            Project.LastModifiedDate = DateTime.Now;
            ProjectHelper.SaveProjects(ProjectHelper.LoadProjects());
        }
    }
}