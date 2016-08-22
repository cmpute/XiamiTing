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
        private void AddListeners()
        {
            PlaylistService.Instance.CurrentIndexChanged += (sender, e) =>
                CurrentSong = e.NewValue == -1 ? SongModel.Null : PlaylistService.Instance.Playlist[e.NewValue];
            PlaybackService.Instance.StateChanged += async (sender, args) =>
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (args.NewValue == MediaPlayerState.Playing)
                            VisualStateManager.GoToState(this, "Playing", true);
                        else
                            VisualStateManager.GoToState(this, "Paused", true);
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
        
    }
}
