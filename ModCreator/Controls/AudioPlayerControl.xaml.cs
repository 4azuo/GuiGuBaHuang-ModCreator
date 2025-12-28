using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModCreator.Controls
{
    /// <summary>
    /// Audio player control for playing audio files with playback controls
    /// </summary>
    public partial class AudioPlayerControl : UserControl
    {
        private DispatcherTimer _audioTimer;
        private bool _isAudioSeeking = false;

        public AudioPlayerControl()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(string),
                typeof(AudioPlayerControl),
                new PropertyMetadata(null, OnSourceChanged));

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioPlayerControl control)
            {
                control.UpdateSource();
            }
        }

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(
                nameof(FileName),
                typeof(string),
                typeof(AudioPlayerControl),
                new PropertyMetadata(string.Empty, OnFileNameChanged));

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        private static void OnFileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioPlayerControl control)
            {
                control.txtFileName.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register(
                nameof(IconColor),
                typeof(System.Windows.Media.Brush),
                typeof(AudioPlayerControl),
                new PropertyMetadata(System.Windows.Media.Brushes.Gray));

        public System.Windows.Media.Brush IconColor
        {
            get => (System.Windows.Media.Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        #endregion

        private void UpdateSource()
        {
            StopAudioPlayback();
            
            if (!string.IsNullOrEmpty(Source))
            {
                audioPlayer.Source = new Uri(Source, UriKind.Absolute);
            }
            else
            {
                audioPlayer.Source = null;
            }
        }

        #region Audio Playback Event Handlers

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

        /// <summary>
        /// Stop audio playback when control is unloaded
        /// </summary>
        public void Cleanup()
        {
            StopAudioPlayback();
            _audioTimer?.Stop();
            _audioTimer = null;
        }
    }
}
