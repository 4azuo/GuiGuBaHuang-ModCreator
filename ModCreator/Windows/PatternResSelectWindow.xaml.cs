using ModCreator.Enums;
using ModCreator.Models;
using ModCreator.WindowData;
using System.ComponentModel;
using System.Windows;

namespace ModCreator.Windows
{
    public partial class PatternResSelectWindow : CWindow<PatternResSelectWindowData>
    {
        public string ResourceFolder { get; set; }
        public GameResourceType ResourceType { get; set; }
        public string SelectedResourcePath { get; private set; }

        public override void OnLoad()
        {
            base.OnLoad();

            WindowData.ResourceType = ResourceType;
            WindowData.ResourceFolder = ResourceFolder;
        }

        /// <summary>
        /// Custom resource selection changed
        /// </summary>
        private void TreeView_CustomResourceSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Stop any playing audio when selection changes
            audioPlayerControl?.Cleanup();

            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedCustomResource = fileItem;
            }
        }

        /// <summary>
        /// Game resource selection changed
        /// </summary>
        private void TreeView_GameResourceSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Stop any playing audio when selection changes
            audioPlayerControl?.Cleanup();
            
            if (e.NewValue is GameResourceItem resourceItem)
            {
                WindowData.SelectedGameResource = resourceItem;
            }
        }

        /// <summary>
        /// Select button click
        /// </summary>
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.IsCustomResourceSelected && WindowData.SelectedCustomResource != null)
            {
                SelectedResourcePath = WindowData.SelectedCustomResource.Name;
            }
            else if (!WindowData.IsCustomResourceSelected && WindowData.SelectedGameResource != null)
            {
                SelectedResourcePath = WindowData.SelectedGameResource.Name;
            }
            else
            {
                WindowData.StatusMessage = "Please select a resource";
                return;
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Cancel button click
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
