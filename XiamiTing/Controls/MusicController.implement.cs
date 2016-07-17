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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace JacobC.Xiami.Controls
{
    //MusicController不负责列表切歌的功能
    public sealed partial class MusicController : UserControl
    {

        DelegateCommand _PlayCommand;
        public DelegateCommand PlayCommand => _PlayCommand ?? (_PlayCommand = new DelegateCommand(() =>
        {
            var service = PlaylistService.Instance;
            LogService.DebugWrite("Play button pressed from App");
            if (service.CurrentPlaying == null)
                if (service.Playlist.Count == 0)
                    return;
                else
                    PlaylistService.Instance.CurrentPlaying = PlaylistService.Instance.Playlist[0];
            var CurrentPlayer = service.CurrentPlayer;
            if (service.IsBackgroundTaskRunning)
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
                    service.StartBackgroundAudioTask();
                }
            }
            else
            {
                service.StartBackgroundAudioTask();
            }
        }));

        private DelegateCommand<object> _PreviousCommand;
        public DelegateCommand<object> PreviousCommand => _PreviousCommand ?? (_PreviousCommand = new DelegateCommand<object>((model) =>
        {
            MessageService.SendMediaMessageToBackground(MediaMessageTypes.SkipPrevious);
            PlaylistService.Instance.CurrentPlaying = PlaylistService.Instance.Playlist[PlaylistService.Instance.Playlist.IndexOf(CurrentSong) - 1];
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
            PlaylistService.Instance.CurrentPlaying = PlaylistService.Instance.Playlist[PlaylistService.Instance.Playlist.IndexOf(CurrentSong) + 1];
            //TODO: 判断是否循环/随机，并且设置Next的可用性

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //nextButton.IsEnabled = false;
        }));
        
    }
}
