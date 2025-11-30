using ModCreator.Helpers;
using ModCreator.WindowData;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        private void OpenProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.Project == null || !Directory.Exists(WindowData.Project.ProjectPath)) return;

            var projectPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject");
            var folderToOpen = Directory.Exists(projectPath) ? projectPath : WindowData.Project.ProjectPath;
            System.Diagnostics.Process.Start("explorer.exe", folderToOpen);
        }
    }
}