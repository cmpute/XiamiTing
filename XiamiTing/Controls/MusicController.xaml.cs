using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using JacobC.Xiami.Services;
using Template10.Mvvm;
using Windows.Media.Playback;

namespace JacobC.Xiami.Controls
{
    /// <summary>
    /// 音乐播放控制器的自定义控件
    /// </summary>
    public sealed partial class MusicController : UserControl
    {
        public static MusicController Instance;

        public MusicController()
        {
            this.InitializeComponent();
            Instance = this;
            this.AddListeners();
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
                IsDownloadBarDisabled = false;
                pservice.StartPlayback();
            }
            else
            {
                IsDownloadBarDisabled = false;
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
            IsDownloadBarDisabled = false;
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
            IsDownloadBarDisabled = false;
            PlaybackService.Instance.SkipNext();
            //TODO: 判断是否循环/随机，并且设置Next的可用性

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //nextButton.IsEnabled = false;
        }));

    }
}
