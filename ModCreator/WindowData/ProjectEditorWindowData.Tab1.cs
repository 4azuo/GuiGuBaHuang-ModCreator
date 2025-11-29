using ModCreator.Attributes;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ModCreator.WindowData
{
    public partial class ProjectEditorWindowData : CWindowData
    {
        internal ModProject _originalProject;

        [NotifyMethod(nameof(LoadProjectData))]
        public ModProject Project { get; set; }

        // Language properties for translation
        public List<Language> SourceLanguages { get; set; } = new List<Language>();
        public Language SelectedSourceLanguage { get; set; }

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
            LoadModResources();

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

        public void LoadModResources()
        {
            var resourcePrefix = "ModCreator.Resources.";

            SourceLanguages = ResourceHelper.ReadEmbeddedResource<List<Language>>($"{resourcePrefix}languages.json");
            EventCategories = ResourceHelper.ReadEmbeddedResource<List<EventCategory>>($"{resourcePrefix}modevent-events.json");
            AvailableConditions = ResourceHelper.ReadEmbeddedResource<List<ConditionInfo>>($"{resourcePrefix}modevent-conditions.json");
            AvailableActions = ResourceHelper.ReadEmbeddedResource<List<ActionInfo>>($"{resourcePrefix}modevent-actions.json");
            CacheTypes = ResourceHelper.ReadEmbeddedResource<List<string>>($"{resourcePrefix}modevent-cachetype.json");
            WorkOnTypes = ResourceHelper.ReadEmbeddedResource<List<string>>($"{resourcePrefix}modevent-workon.json");
            var cats = ResourceHelper.ReadEmbeddedResource<Dictionary<string, List<string>>>($"{resourcePrefix}modevent-cats.json");
            EventCategoryList = cats["EventCategories"];
            ConditionCategoryList = cats["ConditionCategories"];
            ActionCategoryList = cats["ActionCategories"];

            SelectedSourceLanguage = SourceLanguages[0];
        }
    }
}