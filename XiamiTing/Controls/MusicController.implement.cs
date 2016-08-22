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
    //MusicController不负责列表切歌的功能
    public sealed partial class MusicController : UserControl
    {
        private void AddListeners()
        {
            PlaylistService.Instance.CurrentIndexChanged += (sender, e) => CurrentSong = e.NewValue;
            PlaybackService.Instance.CurrentPlayer.CurrentStateChanged += async (sender, args) => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsNowPlaying = sender.CurrentState == MediaPlayerState.Playing);
        }

        DelegateCommand _PlayCommand;
        public DelegateCommand PlayCommand => _PlayCommand ?? (_PlayCommand = new DelegateCommand(() =>
        {
            var lservice = PlaylistService.Instance;
            LogService.DebugWrite("Play button pressed from App");
            if (lservice.CurrentPlaying == null)
                if (lservice.Playlist.Count == 0)
                    return;
                else
                    lservice.CurrentIndex = 0;
            var pservice = PlaybackService.Instance;
            var CurrentPlayer = pservice.CurrentPlayer;
            if (pservice.IsBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == CurrentPlayer.CurrentState)
                {
                    CurrentPlayer.Pause();
                }
                else if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
                {
                    CurrentPlayer.Play();
                }
                else if (MediaPlayerState.Closed == CurrentPlayer.CurrentState)
                {
                    pservice.StartBackgroundAudioTask();
                    pservice.StartPlayback();
                }
            }
            else
            {
                pservice.StartBackgroundAudioTask();
                pservice.StartPlayback();
            }
        }));

        private DelegateCommand<object> _PreviousCommand;
        public DelegateCommand<object> PreviousCommand => _PreviousCommand ?? (_PreviousCommand = new DelegateCommand<object>((model) =>
        {
            MessageService.SendMediaMessageToBackground(MediaMessageTypes.SkipPrevious);
            PlaylistService.Instance.CurrentIndex = PlaylistService.Instance.Playlist.IndexOf(CurrentSong) - 1;
            //TODO: 判断是否循环/随机，并且设置Next的可用性

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //previous.IsEnabled = false;
        }));

        private DelegateCommand<object> _NextCommand;
        public DelegateCommand<object> NextCommand => _NextCommand ?? (_NextCommand = new DelegateCommand<object>((model) =>
        {
            MessageService.SendMediaMessageToBackground(MediaMessageTypes.SkipNext);
            PlaylistService.Instance.CurrentIndex = PlaylistService.Instance.Playlist.IndexOf(CurrentSong);
            //TODO: 判断是否循环/随机，并且设置Next的可用性

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //nextButton.IsEnabled = false;
        }));
        
    }
}
