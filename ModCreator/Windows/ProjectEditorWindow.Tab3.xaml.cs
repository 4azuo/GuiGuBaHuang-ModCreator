using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        private DispatcherTimer _audioTimer;
        private bool _isAudioSeeking = false;
        
        private void TreeView_ImageSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileItem fileItem)
            {
                WindowData.SelectedImageItem = fileItem;
            }
        }

        private void CreateImageFolder_Click(object sender, RoutedEventArgs e)
        {
            var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
            
            string parentPath = imgPath;
            if (WindowData.SelectedImageItem != null)
            {
                parentPath = WindowData.SelectedImageItem.IsFolder
                    ? WindowData.SelectedImageItem.FullPath
                    : Path.GetDirectoryName(WindowData.SelectedImageItem.FullPath);
            }

            var inputWindow = new InputWindow
            {
                Owner = this,
                WindowData = { 
                    WindowTitle = MessageHelper.Get("Messages.Dialogs.CreateFolder.Title"),
                    Label = MessageHelper.Get("Messages.Dialogs.CreateFolder.Label"),
                    InputValue = MessageHelper.Get("Messages.Dialogs.CreateFolder.DefaultValue")
                }
            };

            if (inputWindow.ShowDialog() != true) return;

            var folderName = inputWindow.WindowData.InputValue;

            if (string.IsNullOrWhiteSpace(folderName))
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.FolderNameEmpty"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.FolderNameInvalidChars"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newFolderPath = Path.Combine(parentPath, folderName);

            if (Directory.Exists(newFolderPath))
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.FolderAlreadyExists", folderName), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Directory.CreateDirectory(newFolderPath);
            WindowData.LoadImageFiles();
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.CreatedImageFolder", folderName);
            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.FolderCreated", folderName), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteImageFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.SelectedImageItem == null || !WindowData.SelectedImageItem.IsFolder) return;

            var folderPath = WindowData.SelectedImageItem.FullPath;
            var folderName = WindowData.SelectedImageItem.Name;

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.FolderDoesNotExist", folderName), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                WindowData.LoadImageFiles();
                return;
            }

            var hasContents = Directory.GetFileSystemEntries(folderPath).Length > 0;
            var warningMessage = hasContents
                ? $"Are you sure you want to delete folder '{folderName}' and all its contents?"
                : $"Are you sure you want to delete folder '{folderName}'?";

            var result = MessageBox.Show(warningMessage, "Delete Folder", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Directory.Delete(folderPath, true);
                WindowData.LoadImageFiles();
                WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedImageFolder", folderName);
                MessageBox.Show(MessageHelper.GetFormat("Messages.Success.FolderDeleted", folderName), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = $"Image Files|{string.Join(";", WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}"))}",
                Title = "Select Image to Import",
                Multiselect = true
            })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
                string targetPath = imgPath;
                
                if (WindowData.SelectedImageItem != null)
                {
                    targetPath = WindowData.SelectedImageItem.IsFolder
                        ? WindowData.SelectedImageItem.FullPath
                        : Path.GetDirectoryName(WindowData.SelectedImageItem.FullPath);
                }
                
                Directory.CreateDirectory(targetPath);

                foreach (var file in dialog.FileNames)
                {
                    var destPath = Path.Combine(targetPath, Path.GetFileName(file));
                    File.Copy(file, destPath, true);
                }

                WindowData.LoadImageFiles();
                WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.ImportedImages", dialog.FileNames.Length);
            }
        }

        [SupportedOSPlatform("windows6.1")]
        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedImageFile)) return;

            using (var dialog = new SaveFileDialog
            {
                FileName = WindowData.SelectedImageFile,
                Filter = $"Image Files|{string.Join(";", WindowData.ImageExtensions.Select(ext => $"*{ext.Extension}"))}",
                Title = "Export Image"
            })
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                var sourcePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedImageFile);
                File.Copy(sourcePath, dialog.FileName, true);
                WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.ExportedImage", WindowData.SelectedImageFile);
                MessageBox.Show(MessageHelper.Get("Messages.Success.ImageExported"), MessageHelper.Get("Messages.Success.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WindowData.SelectedImageFile)) return;

            var result = MessageBox.Show(MessageHelper.GetFormat("Messages.Confirmation.DeleteImage", WindowData.SelectedImageFile), MessageHelper.Get("Messages.Confirmation.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg", WindowData.SelectedImageFile);
                var fileName = WindowData.SelectedImageFile;
                WindowData.SelectedImageFile = null;
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    WindowData.LoadImageFiles();
                    WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.DeletedImage", fileName);
                }
            }
        }

        private void OpenModImgFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData?.Project == null) return;
            
            var imgPath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModImg");
            Directory.CreateDirectory(imgPath);
            System.Diagnostics.Process.Start("explorer.exe", imgPath);
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.OpenedModImgFolder", imgPath);
        }

        private void TreeView_GameResourceSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Stop any playing audio when selection changes
            StopAudioPlayback();
            
            if (e.NewValue is GameResourceItem resourceItem)
            {
                WindowData.SelectedGameResourceItem = resourceItem;
            }
        }
        
        #region Audio Playback
        
        private void AudioPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (audioPlayer.NaturalDuration.HasTimeSpan)
            {
                var duration = audioPlayer.NaturalDuration.TimeSpan;
                audioSeekBar.Maximum = duration.TotalSeconds;
                txtTotalTime.Text = FormatTime(duration);
                
                // Initialize audio timer for updating seek bar
                if (_audioTimer == null)
                {
                    _audioTimer = new DispatcherTimer();
                    _audioTimer.Interval = TimeSpan.FromMilliseconds(100);
                    _audioTimer.Tick += AudioTimer_Tick;
                }
            }
        }
        
        private void AudioPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopAudioPlayback();
        }
        
        private void AudioTimer_Tick(object sender, EventArgs e)
        {
            if (audioPlayer.NaturalDuration.HasTimeSpan && !_isAudioSeeking)
            {
                audioSeekBar.Value = audioPlayer.Position.TotalSeconds;
                txtCurrentTime.Text = FormatTime(audioPlayer.Position);
            }
        }
        
        private void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            audioPlayer.Play();
            _audioTimer?.Start();
            btnPlayAudio.IsEnabled = false;
            btnPauseAudio.IsEnabled = true;
            btnStopAudio.IsEnabled = true;
        }
        
        private void PauseAudio_Click(object sender, RoutedEventArgs e)
        {
            audioPlayer.Pause();
            _audioTimer?.Stop();
            btnPlayAudio.IsEnabled = true;
            btnPauseAudio.IsEnabled = false;
            btnStopAudio.IsEnabled = true;
        }
        
        private void StopAudio_Click(object sender, RoutedEventArgs e)
        {
            StopAudioPlayback();
        }
        
        private void StopAudioPlayback()
        {
            audioPlayer.Stop();
            _audioTimer?.Stop();
            audioSeekBar.Value = 0;
            txtCurrentTime.Text = "00:00";
            btnPlayAudio.IsEnabled = true;
            btnPauseAudio.IsEnabled = false;
            btnStopAudio.IsEnabled = false;
        }
        
        private void AudioSeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isAudioSeeking && audioPlayer.NaturalDuration.HasTimeSpan)
            {
                var position = TimeSpan.FromSeconds(audioSeekBar.Value);
                audioPlayer.Position = position;
                txtCurrentTime.Text = FormatTime(position);
            }
        }
        
        private void AudioSeekBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isAudioSeeking = true;
        }
        
        private void AudioSeekBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isAudioSeeking = false;
            if (audioPlayer.NaturalDuration.HasTimeSpan)
            {
                audioPlayer.Position = TimeSpan.FromSeconds(audioSeekBar.Value);
            }
        }
        
        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }
        
        #endregion
    }
}