using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Diagnostics;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Template10.Common;

namespace JacobC.Xiami.Controls
{
    /// <summary>
    /// 播放控制器，相当于MediaTransportControl
    /// </summary>
    public sealed partial class MusicController : UserControl
    {
        DispatcherTimer songtimer = new DispatcherTimer();

        #region EventListeners
        private void AddListeners()
        {
            var inst = PlaybackService.Instance;
            inst.PlaybackSource.CurrentPlayingChanged += PlaybackSource_CurrentPlayingChanged;
            inst.PlaybackSourceChanged += Instance_PlaybackSourceChanged;
            inst.StateChanged += MediaPlayer_StateChanged;
            inst.PlaybackOperated += (sender, e) => IsDownloadBarDisabled = false;
            songtimer.Interval = TimeSpan.FromMilliseconds(300);
            songtimer.Tick += Songtimer_Tick;
            this.Unloaded += (sender, e) => songtimer.Stop();
            ProgressBar.Loaded += (sender, e) =>
                (Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(sender as DependencyObject, 0) as FrameworkElement).RegisterPropertyChangedCallback(TagProperty, (obj, dp) =>
                    IsDraggingSlider = (bool)((obj as FrameworkElement).Tag));
        }
        private void Instance_PlaybackSourceChanged(object sender, Template10.Common.ChangedEventArgs<IPlaylist> e)
        {
            e.OldValue.CurrentPlayingChanged -= PlaybackSource_CurrentPlayingChanged;
            e.NewValue.CurrentPlayingChanged += PlaybackSource_CurrentPlayingChanged;
            this.IsPlayingRadio = (sender as PlaybackService).IsPlayingRadio;
        }
        private async void PlaybackSource_CurrentPlayingChanged(object sender, Template10.Common.ChangedEventArgs<SongModel> e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    CurrentSong = e.NewValue ?? SongModel.Null);
        }
        private async void MediaPlayer_StateChanged(object sender, Template10.Common.ChangedEventArgs<MediaPlayerState> e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.NewValue == MediaPlayerState.Playing || e.NewValue == MediaPlayerState.Buffering)
                {
                    VisualStateManager.GoToState(this, "Playing", true);
                    IsDownloadBarDisabled = true;
                    songtimer.Start();
                }
                else
                {
                    VisualStateManager.GoToState(this, "Paused", true);
                    if (e.NewValue != MediaPlayerState.Opening)
                        songtimer.Stop();
                }
            });
        }
        partial void InternalIsPlayingRadioChanged(ChangedEventArgs<bool> e)
        {
            VisualStateManager.GoToState(this, e.NewValue ? "Radio" : "LocalList", true);
            MessageService.SendMediaMessageToBackground(MediaMessageTypes.SourceTypeChanged, e.NewValue);
        }
        #endregion

        bool _isDragging = false;
        private bool IsDraggingSlider
        {
            get { return _isDragging; }
            set
            {
                if (_isDragging != value)
                {
                    _isDragging = value;
                    if (value)
                        _lastSeekPosition = ProgressBar.Value;//使不滑动的时候也能seek
                    else
                        OnDraggingEnd();
                }
            }
        }

        private void Songtimer_Tick(object sender, object e)
        {
            if (_isDragging)
                return;//拖拽时不改变数值
            var player = PlaybackService.Instance.CurrentPlayer;
            var total = player.NaturalDuration.TotalSeconds;
            if(ProgressBar.Maximum!=total)
            {
                ProgressBar.Maximum = total;
                NaturalDuration.Text = TimeSpanConverter.Covert(total);
            }
            var cur = player.Position.TotalSeconds;
            ProgressBar.Value = cur;
            CurrentPosition.Text = TimeSpanConverter.Covert(cur);
        }

        private double _lastSeekPosition = -1;
        private void ProgressBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isDragging)
            {
                CurrentPosition.Text = TimeSpanConverter.Covert(e.NewValue);
                if (IsSeekingInstant)
                    PlaybackService.Instance.CurrentPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
                else
                    _lastSeekPosition = e.NewValue;
            }
        }

        //拖拽结束时发生
        private void OnDraggingEnd()
        {
            LogService.DebugWrite($"Dragging Seeker Ended at {_lastSeekPosition}", nameof(MusicController));
            if (_lastSeekPosition != -1)
            {
                PlaybackService.Instance.CurrentPlayer.Position = TimeSpan.FromSeconds(_lastSeekPosition);
                _lastSeekPosition = -1;
            }
        }

    }
}
