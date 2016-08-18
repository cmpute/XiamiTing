using JacobC.Xiami.Models;
using static JacobC.Xiami.Services.LogService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Utils;
using System.Threading;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using JacobC.Xiami.ViewModels;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 提供与后台播放程序的通信
    /// </summary>
    public class PlaybackService
    {

        static PlaybackService _instance;
        /// <summary>
        /// 获取当前播放列表实例
        /// </summary>
        public static PlaybackService Instance { get { return _instance ?? (_instance = new PlaybackService()); } }

        SongViewModel _CurrentPlaying = null;
        /// <summary>
        /// 获取当前选中或播放的音轨
        /// </summary>
        public SongViewModel CurrentPlaying
        {
            get
            {
                if (PlaylistService.Instance.Playlist.Count == 0)
                    throw new ArgumentNullException(nameof(CurrentPlaying), "当前播放列表为空，无法获取音轨");
                return _CurrentPlaying;
            }
            set
            {
                if (_CurrentPlaying != value)
                {
                    CurrentIndexChanging.Invoke(this, new ChangedEventArgs<SongViewModel>(_CurrentPlaying, value));
                    InternalCurrentIndexChanging(value);
                    if (_CurrentPlaying != null) _CurrentPlaying.IsPlaying = false;
                    _CurrentPlaying = value;
                    if (value != null) value.IsPlaying = true;
                }
            }
        }
        /// <summary>
        /// 在当前播放的音轨发生改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<SongViewModel>> CurrentIndexChanging;
        private void InternalCurrentIndexChanging(SongViewModel newsong)
        {
            //TODO: 向后台发送消息
        }


        #region Codes for Playback

        private AutoResetEvent backgroundAudioTaskStarted = new AutoResetEvent(false);
        private Dictionary<string, BitmapImage> albumArtCache = new Dictionary<string, BitmapImage>();
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        private bool _isBackgroundTaskRunning = false;
        public bool IsBackgroundTaskRunning
        {
            get
            {
                if (_isBackgroundTaskRunning)
                    return true;

                string value = SettingsService.Instance.Helper.ReadAndReset<string>(nameof(BackgroundTaskState));
                if (value == null)
                    return false;
                else
                {
                    try
                    {
                        _isBackgroundTaskRunning = ExtensionMethods.ParseEnum<BackgroundTaskState>(value) == BackgroundTaskState.Running;
                    }
                    catch (ArgumentException)
                    {
                        _isBackgroundTaskRunning = false;
                    }
                    return _isBackgroundTaskRunning;
                }
            }
        }
        public MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;//重试次数

                while (mp == null && --retryCount >= 0)
                {
                    try
                    {
                        mp = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (mp == null)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }

                return mp;
            }
        }
        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _isBackgroundTaskRunning = false;
            backgroundAudioTaskStarted.Reset();
            //prevButton.IsEnabled = true;
            //nextButton.IsEnabled = true;
            SettingsService.Instance.Helper.Write(nameof(BackgroundTaskState), BackgroundTaskState.Unknown.ToString());
            //playButton.Content = "| |";

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }
                else
                {
                    throw;
                }
            }
        }
        public void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();
            WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    //TODO: 发送第一条音轨
                    MessageService.SendMediaMessageToBackground(MediaMessageTypes.StartTask);
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            }).ContinueWith((task) => {
                if (task.IsCompleted)
                {
                    DebugWrite("Background Audio Task initialized", "MediaPlayer");
                }
                else
                {
                    DebugWrite("Background Audio Task could not initialized due to an error ::" + task.Exception.ToString(), "MediaPlayer");
                }
            });
        }
        public void StartPlayback()
        {
            MessageService.SendMediaMessageToBackground(MediaMessageTypes.StartPlayback);
        }
        private void AddMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
                    ErrorWrite(ex, "BackgroundPlayer");
                    ResetAfterLostBackground();
                }
                else
                {
                    throw;
                }
            }
        }
        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            switch (MessageService.GetTypeOfMediaMessage(e.Data))
            {
                case MediaMessageTypes.TrackChanged:
                    //When foreground app is active change track based on background message
                    await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
                    {
                        // If playback stopped then clear the UI
                        string trackid = MessageService.GetMediaMessage<string>(e.Data);
                        if (trackid == null)
                        {

                            CurrentPlaying = null;
                            //albumArt.Source = null;
                            //txtCurrentTrack.Text = string.Empty;
                            //prevButton.IsEnabled = false;
                            //nextButton.IsEnabled = false;
                            return;
                        }

                        //var songIndex = playlistView.GetSongIndexById(trackid);
                        //var song = playlistView.Songs[songIndex];

                        //// Update list UI
                        //playlistView.SelectedIndex = songIndex;

                        //// Update the album art
                        //albumArt.Source = albumArtCache[song.AlbumArtUri.ToString()];

                        //// Update song title
                        //txtCurrentTrack.Text = song.Title;

                        //// Ensure track buttons are re-enabled since they are disabled when pressed
                        //prevButton.IsEnabled = true;
                        //nextButton.IsEnabled = true;
                    });
                    return;

                case MediaMessageTypes.BackgroundAudioTaskStarted:
                    // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                    // and ready to receive messages
                    DebugWrite("BackgroundAudioTask started");
                    backgroundAudioTaskStarted.Set();
                    return;
                case MediaMessageTypes.SkipNext:
                    SkipNext();
                    return;
                case MediaMessageTypes.SkipPrevious:
                    SkipPrevious();
                    return;
            }
        }
        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
            {
                //// Update state label
                //txtCurrentState.Text = currentState.ToString();

                //// Update controls
                //UpdateTransportControls(currentState);
            });
        }

        #endregion


        public void PlayTrack(SongModel song)
        {
            // Start the background task if it wasn't running
            if (!IsBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            {
                // First update the persisted start track
                SettingsService.Instance.Helper.Write("TrackId", song.MediaUri.ToString());
                SettingsService.Instance.Helper.Write("Position", new TimeSpan().ToString());

                // Start task
                StartBackgroundAudioTask();
            }
            else
            {
                // Switch to the selected track
                MessageService.SendMediaMessageToBackground(MediaMessageTypes.PlaySong, song);
            }
        }
        public void SkipNext()
        { }
        public void SkipPrevious()
        { }
    }
}
