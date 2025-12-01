using ModCreator.Attributes;
using ModCreator.Commons;
using ModCreator.Helpers;
using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace ModCreator.WindowData
{
    public class MainWindowData : CWindowData
    {
        private List<ModProject> _allProjects = new List<ModProject>();
        private string _statusMessage;

        [NotifyMethod(nameof(SaveWorkplacePath))]
        public string WorkplacePath { get; set; }

        [JsonIgnore]
        public List<ModProject> AllProjects
        {
            get => _allProjects;
            set
            {
                _allProjects = value;
                UpdateFilteredProjects(this, null, null, null);
            }
        }

        [JsonIgnore]
        public List<ModProject> FilteredProjects { get; set; } = new List<ModProject>();

        [JsonIgnore]
        public ModProject SelectedProject { get; set; }

        [JsonIgnore]
        [NotifyMethod(nameof(UpdateFilteredProjects))]
        public string SearchText { get; set; }

        [JsonIgnore]
        public string StatusMessage
        {
            get => _statusMessage;
            set => _statusMessage = $"{DateTime.Now:HH:mm:ss} - {value}";
        }

        [JsonIgnore]
        public int TotalCount => FilteredProjects?.Count ?? 0;

        [JsonIgnore]
        public bool HasSelectedProject => SelectedProject != null;

        [JsonIgnore]
        public bool HasNoSelectedProject => SelectedProject == null;

        [JsonIgnore]
        public bool IsSelectedProjectValid => SelectedProject?.State == Enums.ProjectState.Valid;

        [JsonIgnore]
        public string ProjectName => SelectedProject?.ProjectName ?? "";

        [JsonIgnore]
        public string ProjectId => SelectedProject?.ProjectId ?? "";

        [JsonIgnore]
        public string ProjectPath => SelectedProject?.ProjectPath ?? "";

        [JsonIgnore]
        public string Description => string.IsNullOrEmpty(SelectedProject?.Description) ? "-" : SelectedProject.Description;

        [JsonIgnore]
        public string Author => SelectedProject?.Author ?? "";

        [JsonIgnore]
        public string TitleImg => SelectedProject?.TitleImg ?? "";

        public override void OnLoad()
        {
            LoadProjects();
        }

        public void LoadProjects()
        {
            AllProjects = ProjectHelper.LoadProjects();

            foreach (var project in AllProjects)
            {
                project.State = Directory.Exists(project.ProjectPath) 
                    ? Enums.ProjectState.Valid 
                    : Enums.ProjectState.ProjectNotFound;
            }

            StatusMessage = MessageHelper.GetFormat("Messages.Success.LoadedProjects", AllProjects.Count);
        }

        public ModProject CreateProject(string projectName, string targetDirectory, string description)
        {
            var newProject = ProjectHelper.CreateProject(projectName, targetDirectory, description);
            AllProjects.Add(newProject);
            ProjectHelper.SaveProjects(AllProjects);
            UpdateFilteredProjects(this, null, null, null);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.CreatedProject", projectName);
            return newProject;
        }

        public void DeleteProject(bool deleteFiles)
        {
            if (SelectedProject == null)
                return;

            var projectName = SelectedProject.ProjectName;
            ProjectHelper.DeleteProject(SelectedProject, deleteFiles);
            AllProjects.Remove(SelectedProject);
            ProjectHelper.SaveProjects(AllProjects);
            SelectedProject = null;
            UpdateFilteredProjects(this, null, null, null);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedProject", projectName);
        }

        public void OpenProjectFolder()
        {
            if (SelectedProject == null)
                return;

            ProjectHelper.OpenProjectFolder(SelectedProject);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.OpenedFolder", SelectedProject.ProjectName);
        }

        public void UpdateFilteredProjects(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (AllProjects == null)
            {
                FilteredProjects = new List<ModProject>();
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProjects = new List<ModProject>(AllProjects);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                FilteredProjects = AllProjects.Where(p =>
                    p.ProjectName.ToLower().Contains(searchLower) ||
                    p.ProjectId.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.ToLower().Contains(searchLower))
                ).ToList();
            }
        }

        public void SaveWorkplacePath(object obj, PropertyInfo prop, object oldValue, object newValue)
        {
            if (string.IsNullOrWhiteSpace(WorkplacePath))
                return;

            if (!Directory.Exists(WorkplacePath))
            {
                StatusMessage = MessageHelper.GetFormat("Messages.Error.WorkplacePathNotFound", WorkplacePath);
                return;
            }

            Properties.Settings.Default.WorkplacePath = WorkplacePath;
            Properties.Settings.Default.Save();
            StatusMessage = MessageHelper.GetFormat("Messages.Success.WorkplacePathSaved", WorkplacePath);
        }

        public void BuildProject()
        {
            if (SelectedProject == null) return;

            var scriptPath = Path.Combine(SelectedProject.ProjectPath, ".vscode", "rebuild-project.ps1");
            if (!File.Exists(scriptPath))
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Error.BuildScriptNotFound", scriptPath),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -NoExit -File \"{scriptPath}\"",
                UseShellExecute = true,
                WorkingDirectory = SelectedProject.ProjectPath
            };

            var process = System.Diagnostics.Process.Start(processInfo);
            StatusMessage = MessageHelper.Get("Messages.Success.ProjectBuilt");
        }

        public void DeployProject()
        {
            if (SelectedProject == null) return;

            var scriptPath = Path.Combine(SelectedProject.ProjectPath, ".vscode", "rebuild-and-deploy.ps1");
            if (!File.Exists(scriptPath))
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Error.DeployScriptNotFound", scriptPath),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -NoExit -File \"{scriptPath}\"",
                UseShellExecute = true,
                WorkingDirectory = SelectedProject.ProjectPath
            };

            var process = System.Diagnostics.Process.Start(processInfo);
            StatusMessage = MessageHelper.Get("Messages.Success.ProjectDeployed");
        }

        public void PublishProject(string targetPath)
        {
            if (SelectedProject == null) return;

            var scriptPath = Path.Combine(SelectedProject.ProjectPath, ".vscode", "rebuild-project.ps1");
            if (!File.Exists(scriptPath))
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Error.BuildScriptNotFound", scriptPath),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = SelectedProject.ProjectPath
            };

            var process = System.Diagnostics.Process.Start(processInfo);
            process.WaitForExit();
                    
            if (process.ExitCode != 0)
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.Get("Messages.Error.BuildFailed"),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var sourceFolder = Path.Combine(SelectedProject.ProjectPath, "debug", $"Mod_{SelectedProject.ProjectId}");
            if (!Directory.Exists(sourceFolder))
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Error.PublishSourceNotFound", sourceFolder),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            if (File.Exists(targetPath))
                File.Delete(targetPath);

            System.IO.Compression.ZipFile.CreateFromDirectory(sourceFolder, targetPath, System.IO.Compression.CompressionLevel.Optimal, false);
            StatusMessage = MessageHelper.GetFormat("Messages.Success.ProjectPublished", targetPath);
            System.Windows.MessageBox.Show(
                MessageHelper.GetFormat("Messages.Success.ProjectPublished", targetPath),
                MessageHelper.Get("Messages.Success.Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        public void CopyToModExportData()
        {
            if (SelectedProject == null) return;

            var scriptPath = Path.Combine(SelectedProject.ProjectPath, ".vscode", "rebuild-project.ps1");
            if (!File.Exists(scriptPath))
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Error.BuildScriptNotFound", scriptPath),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = SelectedProject.ProjectPath
            };

            var process = System.Diagnostics.Process.Start(processInfo);
            process.WaitForExit();
                
            if (process.ExitCode != 0)
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.Get("Messages.Error.BuildFailed"),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var sourceFolder = Path.Combine(SelectedProject.ProjectPath, "debug", $"Mod_{SelectedProject.ProjectId}");
            if (!Directory.Exists(sourceFolder))
            {
                System.Windows.MessageBox.Show(
                    MessageHelper.GetFormat("Messages.Error.PublishSourceNotFound", sourceFolder),
                    MessageHelper.Get("Messages.Error.Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var targetFolder = Path.Combine(Constants.GameFolderPath, "ModExportData", $"Mod_{SelectedProject.ProjectId}");
                
            if (Directory.Exists(targetFolder))
                Directory.Delete(targetFolder, true);

            Helpers.FileHelper.CopyDirectory(sourceFolder, targetFolder);
                
            StatusMessage = MessageHelper.GetFormat("Messages.Success.CopiedToModExportData", targetFolder);
            System.Windows.MessageBox.Show(
                MessageHelper.GetFormat("Messages.Success.CopiedToModExportData", targetFolder),
                MessageHelper.Get("Messages.Success.Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}