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

        #region PlaySource
        bool _isPlayingRadio = false;
        RadioService radio = null;
        /// <summary>
        /// 获取是否正在播放电台
        /// </summary>
        public bool IsPlayingRadio { get { return _isPlayingRadio; } }
        public IPlaylist PlaybackSource => radio ?? (IPlaylist)PlaylistService.Instance;
        /// <summary>
        /// 设置播放内容为播放列表或电台
        /// </summary>
        /// <param name="source">播放来源，null代表播放列表</param>
        public void SetPlaybackSource(RadioService source)
        {
            if (radio?.Radio == source?.Radio)
                return;
            var e = new ChangedEventArgs<IPlaylist>(radio ?? (IPlaylist)PlaylistService.Instance, source ?? (IPlaylist)PlaylistService.Instance);
            if (source == null)
            {
                radio = null;
                _isPlayingRadio = false;
            }
            else
            {
                radio = source;
                _isPlayingRadio = true;
            }
            PlaybackSourceChanged?.Invoke(this, e);
            InternalPlaybackSourceChanged(_isPlayingRadio);
            SkipNext();
        }
        /// <summary>
        /// 当播放源发生改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<IPlaylist>> PlaybackSourceChanged;
        private void InternalPlaybackSourceChanged(bool isPlayingRadio)
        {

        }
        #endregion

        #region Codes for BackgroundTask

        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        private bool _isBackgroundTaskRunning = false;
        public bool IsBackgroundTaskRunning
        {
            get
            {
                if (_isBackgroundTaskRunning)
                    return true;

                string value = SettingsService.Playback.ReadAndReset<string>(nameof(BackgroundTaskState));
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
            SettingsService.Playback.Write(nameof(BackgroundTaskState), BackgroundTaskState.Unknown.ToString());
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
            StateChanged?.Invoke(null, e);
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
        public async void PlayTrack(SongModel song)
        {
            if (_isPlayingRadio)
                throw new InvalidOperationException("在播放歌曲前应停止电台播放");
            if (string.IsNullOrEmpty(song.Album?.Art?.Host))
                await Net.WebApi.Instance.GetSongInfo(song);
            if (!PlaylistService.Instance.Contains(song))
            {
                PlaylistService.Instance.Add(song);
                PlayTrack(PlaylistService.Instance.Count - 1);
            }
            else
                PlayTrack(PlaylistService.Instance.IndexOf(song));
        }
        /// <summary>
        /// 播放列表指定位置的歌曲
        /// </summary>
        /// <param name="trackIndex">播放的歌曲位置</param>
        public void PlayTrack(int trackIndex)
        {
            if (_isPlayingRadio)
                throw new InvalidOperationException("在播放歌曲前应停止电台播放");
            if (PlayTrackInternal(PlaylistService.Instance[trackIndex]))
                PlaylistService.Instance.CurrentIndex = trackIndex;
        }
        /// <summary>
        /// 播放当前轨
        /// </summary>
        public void PlayTrack()
        {
            if (_isPlayingRadio)
                PlayTrackInternal(radio.CurrentPlaying);
            else
            {
                var lservice = PlaylistService.Instance;
                if (lservice.CurrentPlaying == null)
                    if (lservice.Count == 0)
                        return;
                    else
                        lservice.CurrentIndex = 0;
                PlayTrack(lservice.CurrentIndex);
            }
        }
        /// <returns>是否成功开始播放</returns>
        private bool PlayTrackInternal(SongModel song)
        {
            PlaybackOperated?.Invoke(song, null);
            // Start the background task if it wasn't running
            //if (!IsBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            if (!IsBackgroundTaskRunning)
            {
                // First update the persisted start track
                SettingsService.Playback.Write("TrackId", song.MediaUri.ToString());
                SettingsService.Playback.Write("Position", new TimeSpan().ToString());

                // Start task
                StartBackgroundAudioTask();
                return false;
            }
            else
            {
                //TODO: 增加如果播放不成功则刷新地址
                if (song.MediaUri == null)
                    ExtensionMethods.InvokeAndWait(async () => song.MediaUri = new Uri(await Net.DataApi.GetDownloadLink(song, false)));
                MessageService.SendMediaMessageToBackground(MediaMessageTypes.SetSong, song);
                MessageService.SendMediaMessageToBackground(MediaMessageTypes.StartPlayback);
                return true;
                
            }
        }

        /// <summary>
        /// 播放下一首
        /// </summary>
        public async void SkipNext()
        {
            //TODO:判断播放模式
            //TODO:判断暂停状态
            //TODO:判断是播放列表还是电台
            PlaybackOperated?.Invoke(null, null);
            if (_isPlayingRadio)
            {
                await radio.PlayNext();
                PlayTrackInternal(radio.CurrentPlaying);
            }
            else
            {
                var list = PlaylistService.Instance;
                if (list.CurrentIndex == list.Count - 1)
                    PlayTrack(0);
                else
                    PlayTrack(list.CurrentIndex + 1);
            }
        }
        /// <summary>
        /// 播放上一首
        /// </summary>
        public void SkipPrevious()
        {
            if(_isPlayingRadio)
                throw new InvalidOperationException("电台不支持上一首功能");
            PlaybackOperated?.Invoke(null, null);
            var list = PlaylistService.Instance;
            if (list.CurrentIndex == 0)
                PlayTrack(list.Count - 1);
            else
                PlayTrack(list.CurrentIndex - 1);
        }

        #endregion

        #region Model Playing Methods
        /// <summary>
        /// 播放虾米内容，自动判断对象类型
        /// </summary>
        /// <param name="model"></param>
        public void PlayModel(object model)
        {
            PlaybackOperated?.Invoke(model, null);
            if (model == null)
                return;
            if (model is AlbumModel)
                PlayAlbum(model as AlbumModel);
        }
        /// <summary>
        /// 播放专辑
        /// </summary>
        public async void PlayAlbum(AlbumModel album)
        {
            if (album == null)
                return;
            if (album.SongList == null)
                await Net.WebApi.Instance.GetAlbumInfo(album);
            PlaylistService.Instance.Clear();
            PlaylistService.Instance.AddAlbum(album);
            PlayTrack();
        }
        #endregion

        /// <summary>
        /// 在Playback相关操作进行时发生
        /// </summary>
        public event EventHandler PlaybackOperated;
        
    }
}
