using ModCreator.Helpers;
using ModCreator.Models;
using System;
using System.IO;

namespace ModCreator
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Root directory of the project
        /// </summary>
        public static string RootDir => SettingHelper.TryGet("rootDir", @"C:/git/GuiGuBaHuang-ModLib/");

        /// <summary>
        /// Steam Workshop directory
        /// </summary>
        public static string SteamWorkshopDir => SettingHelper.TryGet("steamWorkshopDir", @"C:/Program Files (x86)/Steam/steamapps/workshop/content/1468810");

        /// <summary>
        /// Game folder directory
        /// </summary>
        public static string GameFolderPath => SettingHelper.TryGet("gameFolderPath", @"C:/Program Files (x86)/Steam/steamapps/common/鬼谷八荒");
        
        /// <summary>
        /// Documentation directory (.github/docs)
        /// </summary>
        public static readonly string DocsDir = Path.GetFullPath(Path.Combine(RootDir, ".github", "docs"));

        /// <summary>
        /// Resources directory (bin/*/Resources)
        /// </summary>
        public static readonly string ResourcesDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"));

        /// <summary>
        /// Log directory (Logs in root)
        /// </summary>
        public static readonly string LogsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

        public static readonly EventActionBase EventActionRootElement = new EventActionBase
        {
            Category = "Root",
            Name = "Root",
            DisplayName = "Root",
            Code = "Root",
            Description = "The root element of event-actions.",
            IsCanAddChild = true,
        };

        /// <summary>
        /// Default workplace directory name
        /// </summary>
        public const string DEFAULT_WORKPLACE_DIR = "GuiGuBaHuang-ModProjects";
    }
}