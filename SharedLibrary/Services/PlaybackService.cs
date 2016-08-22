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

        #region Codes for BackgroundTask

        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        private bool _isBackgroundTaskRunning = false;
        public bool IsBackgroundTaskRunning
        {
            get
            {
                if (_isBackgroundTaskRunning)
                    return true;

                string value = SettingsService.Instance.Playback.ReadAndReset<string>(nameof(BackgroundTaskState));
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
                            throw;
                    }
                }

                if (mp == null)
                    throw new Exception("Failed to get a MediaPlayer instance.");
                return mp;
            }
        }
        //TODO: 完善后台获取失败的方法
        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _isBackgroundTaskRunning = false;
            //prevButton.IsEnabled = true;
            //nextButton.IsEnabled = true;
            SettingsService.Instance.Playback.Write(nameof(BackgroundTaskState), BackgroundTaskState.Unknown.ToString());
            //playButton.Content = "||";

            AddMessageHandler();
        }
        public void StartBackgroundAudioTask()
        {
            CurrentPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            AddMessageHandler();
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            //TODO: 发送第一条音轨
        }
        public void StartPlayback()
        {
            MessageService.SendMediaMessageToBackground(MediaMessageTypes.StartPlayback);
        }
        private void AddMessageHandler()
        {
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
                    throw;
            }
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            await Task.CompletedTask;
            switch (MessageService.GetTypeOfMediaMessage(e.Data))
            {
                /*
                case MediaMessageTypes.TrackChanged:
                    //When foreground app is active change track based on background message
                    await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
                    {
                        // If playback stopped then clear the UI
                        string trackid = MessageService.GetMediaMessage<string>(e.Data);
                        if (trackid == null)
                        {

                            PlaylistService.Instance.CurrentPlaying = null;
                            //albumArt.Source = null;
                            //txtCurrentTrack.Text = string.Empty;
                            //prevButton.IsEnabled = false;
                            //nextButton.IsEnabled = false;
                            return;
                        }
                    });
                    return;
                    */
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
            await Task.CompletedTask;
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            //await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
            //{
            //    // Update state label
            //    txtCurrentState.Text = currentState.ToString();

            //    // Update controls
            //    UpdateTransportControls(currentState);
            //});
        }

        #endregion

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var e = new ChangedEventArgs<MediaPlayerState>(CurrentState, sender.CurrentState);
            StateChanged.Invoke(null, e);
            CurrentState = sender.CurrentState;
        }

        /// <summary>
        /// 获取后台播放器当前的状态
        /// </summary>
        public MediaPlayerState CurrentState { get; set; }
        /// <summary>
        /// 当<see cref="CurrentState"/>属性发生改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<MediaPlayerState>> StateChanged;
        
        #region Public Controlling Methods 

        /// <summary>
        /// 播放指定歌曲
        /// </summary>
        /// <param name="song">需要播放的歌曲，如果不在列表中的话将加入列表</param>
        public void PlayTrack(SongModel song)
        {
            if (!PlaylistService.Instance.Playlist.Contains(song))
            {
                PlaylistService.Instance.Playlist.Add(song);
                PlayTrack(PlaylistService.Instance.Playlist.Count - 1);
            }
            else
                PlayTrack(PlaylistService.Instance.Playlist.IndexOf(song));
        }
        /// <summary>
        /// 播放列表指定位置的歌曲
        /// </summary>
        /// <param name="trackIndex">播放的歌曲位置</param>
        public void PlayTrack(int trackIndex)
        {
            if(PlayTrackInternal(PlaylistService.Instance.Playlist[trackIndex]))
                PlaylistService.Instance.CurrentIndex = trackIndex;
        }
        /// <summary>
        /// 播放当前轨
        /// </summary>
        public void PlayTrack()
        {
            var lservice = PlaylistService.Instance;
            if (lservice.CurrentPlaying == null)
                if (lservice.Playlist.Count == 0)
                    return;
                else
                    lservice.CurrentIndex = 0;
            PlayTrack(lservice.CurrentIndex);
        }
        /// <returns>是否成功开始播放</returns>
        private bool PlayTrackInternal(SongModel song)
        {
            // Start the background task if it wasn't running
            //if (!IsBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            if (!IsBackgroundTaskRunning)
            {
                // First update the persisted start track
                SettingsService.Instance.Playback.Write("TrackId", song.MediaUri.ToString());
                SettingsService.Instance.Playback.Write("Position", new TimeSpan().ToString());

                // Start task
                StartBackgroundAudioTask();
                return false;
            }
            else
            {
                MessageService.SendMediaMessageToBackground(MediaMessageTypes.SetSong, song);
                MessageService.SendMediaMessageToBackground(MediaMessageTypes.StartPlayback);
                return true;
                
            }
        }

        /// <summary>
        /// 播放下一首
        /// </summary>
        public void SkipNext()
        {
            //TODO:判断播放模式
            //TODO:判断暂停状态
            //TODO:判断是播放列表还是电台
            var list = PlaylistService.Instance;
            if (list.CurrentIndex == list.Playlist.Count - 1)
                PlayTrack(0);
            else
                PlayTrack(list.CurrentIndex + 1);
        }
        /// <summary>
        /// 播放上一首
        /// </summary>
        public void SkipPrevious()
        {
            var list = PlaylistService.Instance;
            if (list.CurrentIndex == 0)
                PlayTrack(list.Playlist.Count - 1);
            else
                PlayTrack(list.CurrentIndex - 1);
        }

        #endregion

        /// <summary>
        /// 设置播放内容为播放列表或电台
        /// </summary>
        /// <param name="source">播放来源，null代表播放列表</param>
        public void SetPlaybackSource(RadioService source)
        {
            throw new NotImplementedException();
        }
    }
}
