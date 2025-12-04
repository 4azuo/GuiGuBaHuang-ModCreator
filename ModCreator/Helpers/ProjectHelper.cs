using ModCreator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for project operations
    /// </summary>
    public static class ProjectHelper
    {
        private const string PROJECTS_DATA_FILE = "projects.json";
        private const string PROJECT_TEMPLATE_NAME = "ModProject_0hKMNX";
        
        /// <summary>
        /// Get the projects data file path
        /// </summary>
        public static string GetProjectsDataFilePath()
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PROJECTS_DATA_FILE));
        }

        /// <summary>
        /// Get the project template path
        /// </summary>
        public static string GetProjectTemplatePath()
        {
            return Path.GetFullPath(Path.Combine(Constants.ResourcesDir, PROJECT_TEMPLATE_NAME));
        }

        /// <summary>
        /// Load all projects from individual project.json files in WorkplacePath
        /// </summary>
        public static List<ModProject> LoadProjects()
        {
            var projects = new List<ModProject>();
            var workplacePath = Properties.Settings.Default.WorkplacePath;

            if (string.IsNullOrEmpty(workplacePath) || !Directory.Exists(workplacePath))
                return projects;

            try
            {
                // Get all subdirectories in workplace
                var projectDirs = Directory.GetDirectories(workplacePath);

                foreach (var projectDir in projectDirs)
                {
                    var projectFilePath = Path.Combine(projectDir, "project.json");
                    if (File.Exists(projectFilePath))
                    {
                        try
                        {
                            var json = FileHelper.ReadTextFile(projectFilePath);
                            var project = JsonConvert.DeserializeObject<ModProject>(json);
                            if (project != null)
                            {
                                projects.Add(project);
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugHelper.Warning($"Failed to load project from {projectFilePath}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Error($"Error loading projects: {ex.Message}");
            }

            return projects;
        }

        /// <summary>
        /// Save individual project to its project.json file
        /// </summary>
        public static void SaveProject(ModProject project)
        {
            if (project == null || string.IsNullOrEmpty(project.ProjectPath))
                return;

            try
            {
                var projectFilePath = Path.Combine(project.ProjectPath, "project.json");
                var json = JsonConvert.SerializeObject(project, Formatting.Indented);
                FileHelper.WriteTextFile(projectFilePath, json);
            }
            catch (Exception ex)
            {
                DebugHelper.Error($"Failed to save project {project.ProjectName}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new project from template
        /// </summary>
        public static ModProject CreateProject(string projectName, string targetDirectory, string description = "", string author = "")
        {
            var templatePath = GetProjectTemplatePath();
            if (!Directory.Exists(templatePath))
            {
                throw new DirectoryNotFoundException($"Project template not found at: {templatePath}");
            }

            // Generate project ID
            var projectId = FileHelper.GenerateProjectId();
            var projectFolderName = $"ModProject_{projectId}";
            var projectPath = Path.Combine(targetDirectory, projectFolderName);

            // Check if project already exists
            if (Directory.Exists(projectPath))
            {
                throw new InvalidOperationException($"Project directory already exists: {projectPath}");
            }

            // Create project object
            var project = new ModProject
            {
                ProjectId = projectId,
                ProjectName = projectName,
                ProjectPath = projectPath,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = description,
                Author = author,
                TitleImg = Path.GetFullPath(Path.Combine(projectPath, "ModProject", "ModProjectPreview.png")),
                GlobalVariables =
                [
                    new() {
                        Name = "MOD_VERSION",
                        Type = "string",
                        Value = "1.0.0",
                        Description = "Mod version"
                    }
                ]
            };

            // Copy template
            FileHelper.CopyDirectory(templatePath, projectPath);

            // Apply replacements based on project-replacements.json
            ApplyProjectReplacements(projectPath, project);

            // Save project.json
            SaveProject(project);

            return project;
        }

        /// <summary>
        /// Apply replacements to project files based on project-replacements.json
        /// </summary>
        private static void ApplyProjectReplacements(string projectPath, ModProject project)
        {
            try
            {
                // Read replacements configuration from embedded resource
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "ModCreator.Resources.project-replacements.json";
                
                string json;
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        DebugHelper.Warning($"Embedded resource not found: {resourceName}");
                        return;
                    }
                    
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }
                }

                var config = JsonConvert.DeserializeObject<ProjectReplacementsConfig>(json);
                if (config == null || config.Position == null || config.KeyValues == null)
                {
                    DebugHelper.Warning("Invalid replacements configuration");
                    return;
                }

                // Process each file in the Position list
                foreach (var relativePath in config.Position)
                {
                    var filePath = Path.Combine(projectPath, relativePath);
                    if (!File.Exists(filePath))
                    {
                        DebugHelper.Warning($"File not found for replacement: {filePath}");
                        continue;
                    }

                    // Read file content
                    var content = FileHelper.ReadTextFile(filePath);

                    // Apply all key-value replacements
                    foreach (var kvp in config.KeyValues)
                    {
                        var placeholder = kvp.Key;
                        var propertyPath = kvp.Value; // e.g., "ModCreator.Models.ModProject.ProjectId"
                        
                        // Get the replacement value based on property path
                        var replacementValue = GetReplacementValue(propertyPath, project);
                        
                        // Replace in content
                        content = content.Replace(placeholder, replacementValue);
                    }

                    // Write back to file
                    FileHelper.WriteTextFile(filePath, content);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Error($"Error applying project replacements: {ex.Message}");
            }
        }

        /// <summary>
        /// Get replacement value based on property path
        /// </summary>
        private static string GetReplacementValue(string propertyPath, ModProject project)
        {
            var value = propertyPath switch
            {
                "Constants.SteamWorkshopDir" => Constants.SteamWorkshopDir,
                _ => project.GetValue(propertyPath, true).Parse<string>() ?? string.Empty
            };

            // Format backslashes for file paths/URLs in string literals
            if (!string.IsNullOrEmpty(value) && value.Contains('\\'))
            {
                value = value.Replace("\\", "\\\\");
            }

            return value;
        }

        /// <summary>
        /// Update project info
        /// </summary>
        public static void UpdateProject(ModProject project)
        {
            //Todo
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        public static void DeleteProject(ModProject project, bool deleteFiles = false)
        {
            if (project == null)
                return;

            try
            {
                if (deleteFiles && Directory.Exists(project.ProjectPath))
                {
                    FileHelper.DeleteFolderSafe(project.ProjectPath);
                }
                else
                {
                    // Just delete the project.json file
                    var projectFilePath = Path.Combine(project.ProjectPath, "project.json");
                    if (File.Exists(projectFilePath))
                    {
                        File.Delete(projectFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Error($"Failed to delete project {project.ProjectName}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if project path is valid
        /// </summary>
        public static bool IsProjectValid(ModProject project)
        {
            return project != null && 
                   !string.IsNullOrEmpty(project.ProjectPath) && 
                   Directory.Exists(project.ProjectPath);
        }

        /// <summary>
        /// Open project folder in explorer
        /// </summary>
        public static void OpenProjectFolder(ModProject project)
        {
            if (IsProjectValid(project))
            {
                System.Diagnostics.Process.Start("explorer.exe", project.ProjectPath);
            }
        }
    }
}
