using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace JacobC.Xiami.Controls
{
    /// <summary>
    /// 播放控制器，相当于MediaTransportControl
    /// </summary>
    public sealed partial class MusicController : UserControl
    {
        DispatcherTimer songtimer = new DispatcherTimer();
        bool timerblocked = false;

        private void AddListeners()
        {
            PlaylistService.Instance.CurrentIndexChanged += (sender, e) =>
                CurrentSong = e.NewValue == -1 ? SongModel.Null : PlaylistService.Instance.Playlist[e.NewValue];
            PlaybackService.Instance.StateChanged += MediaPlayer_StateChanged;
            songtimer.Interval = TimeSpan.FromMilliseconds(300);
            songtimer.Tick += Songtimer_Tick;
            this.Unloaded += (sender, e) => songtimer.Stop();
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

        DelegateCommand _PlayCommand;
        public DelegateCommand PlayCommand => _PlayCommand ?? (_PlayCommand = new DelegateCommand(() =>
        {
            var pservice = PlaybackService.Instance;
            var CurrentPlayer = pservice.CurrentPlayer;
            if (MediaPlayerState.Playing == CurrentPlayer.CurrentState)
            {
                CurrentPlayer.Pause();
            }
            else if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
            {
                pservice.StartPlayback();
            }
            else
            {
                pservice.PlayTrack();
                //else if (MediaPlayerState.Closed == CurrentPlayer.CurrentState)
                //{
                //    pservice.StartBackgroundAudioTask();
                //    pservice.StartPlayback();
                //}
            }
        }));

        private DelegateCommand<object> _PreviousCommand;
        public DelegateCommand<object> PreviousCommand => _PreviousCommand ?? (_PreviousCommand = new DelegateCommand<object>((model) =>
        {
            PlaybackService.Instance.SkipPrevious();
            //TODO: 判断是否循环/随机，并且设置Next的可用性

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //previous.IsEnabled = false;
        }));

        private DelegateCommand<object> _NextCommand;
        public DelegateCommand<object> NextCommand => _NextCommand ?? (_NextCommand = new DelegateCommand<object>((model) =>
        {
            PlaybackService.Instance.SkipNext();
            //TODO: 判断是否循环/随机，并且设置Next的可用性

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //nextButton.IsEnabled = false;
        }));

        private void Songtimer_Tick(object sender, object e)
        {
            timerblocked = true;
            var player = PlaybackService.Instance.CurrentPlayer;
            ProgressBar.Maximum = player.NaturalDuration.TotalSeconds;
            ProgressBar.Value = player.Position.TotalSeconds;
            //Debug.WriteLine(player.Position.TotalSeconds);
            timerblocked = false;
        }

    }
}
