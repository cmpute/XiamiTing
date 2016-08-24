using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Diagnostics;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace JacobC.Xiami.Controls
{
    /// <summary>
    /// 播放控制器，相当于MediaTransportControl
    /// </summary>
    public sealed partial class MusicController : UserControl
    {
        DispatcherTimer songtimer = new DispatcherTimer();

        private void AddListeners()
        {
            PlaylistService.Instance.CurrentIndexChanged += async (sender, e) =>
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    CurrentSong = e.NewValue == -1 ? SongModel.Null : PlaylistService.Instance[e.NewValue]);
            PlaybackService.Instance.StateChanged += MediaPlayer_StateChanged;
            songtimer.Interval = TimeSpan.FromMilliseconds(300);
            songtimer.Tick += Songtimer_Tick;
            this.Unloaded += (sender, e) => songtimer.Stop();
            ProgressBar.Loaded += (sender, e) =>
                (Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(sender as DependencyObject, 0) as FrameworkElement).RegisterPropertyChangedCallback(TagProperty, (obj, dp) =>
                    IsDraggingSlider = (bool)((obj as FrameworkElement).Tag));
        }

        private async void MediaPlayer_StateChanged(object sender, Template10.Common.ChangedEventArgs<MediaPlayerState> e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.NewValue == MediaPlayerState.Playing || e.NewValue == MediaPlayerState.Buffering)
                {
                    VisualStateManager.GoToState(this, "Playing", true);
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

        bool _isDragging = false;
        private bool IsDraggingSlider
        {
            get { return _isDragging; }
            set
            {
                if (_isDragging != value)
                {
                    _isDragging = value;
                    if (!value)
                        OnDraggingEnd();
                }
            }
        }

        private void Songtimer_Tick(object sender, object e)
        {
            if (_isDragging)
                return;//拖拽时不改变数值
            var player = PlaybackService.Instance.CurrentPlayer;
            ProgressBar.Maximum = player.NaturalDuration.TotalSeconds;
            ProgressBar.Value = player.Position.TotalSeconds;
            //TODO: Tick更改时间指示文字
        }

        private double _lastSeekPosition = -1;
        private void ProgressBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isDragging)
            {
                //TODO: 拖拽时更改时间指示文字
                _lastSeekPosition = e.NewValue;
            }
        }

        //拖拽结束时发生 TODO: 验证是否生效
        private void OnDraggingEnd()
        {
            if (_lastSeekPosition != -1)
            {
                PlaybackService.Instance.CurrentPlayer.Position = TimeSpan.FromSeconds(_lastSeekPosition);
                _lastSeekPosition = -1;
            }
        }

    }
}
