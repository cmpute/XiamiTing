using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using JacobC.Xiami.Models;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 提供音频播放服务的后台任务
    /// </summary>
    public sealed class AudioTask : IBackgroundTask
    {
 
        #region Private fields, properties
        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string AlbumArtKey = "albumart";
        private SystemMediaTransportControls smtc;
        private MediaPlaybackList playbackList = new MediaPlaybackList();
        private BackgroundTaskDeferral deferral; // 保持任务活动
        private AppState foregroundAppState = AppState.Unknown;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private bool playbackStartedPreviously = false;
        Template10.Services.SettingsService.SettingsHelper settinghelper = SettingsService.Instance.Helper;
        #endregion

        #region Helper methods
        Uri GetCurrentTrackId()
        {
            if (playbackList == null)
                return null;

            return GetTrackId(playbackList.CurrentItem);
        }

        Uri GetTrackId(MediaPlaybackItem item)
        {
            if (item == null)
                return null; //没有播放中的音轨

            return item.Source.CustomProperties[TrackIdKey] as Uri;
        }
        #endregion

        #region IBackgroundTask and IBackgroundTaskInstance Interface Members and handlers

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");

            // 初始化SystemMediaTrasportControls(SMTC)，嵌入UVC
            // UI和UVC需要在程序被终止时更新，因此SMTC需要配置并且从后台任务获得更新
            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += smtc_ButtonPressed;
            smtc.PropertyChanged += smtc_PropertyChanged;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;

            foregroundAppState = settinghelper.ReadAndReset(nameof(AppState), AppState.Unknown); //读取APP状态
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged; //为播放器添加后台控制句柄
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground; //初始化消息通道

            if (foregroundAppState != AppState.Suspended)
                MessageService.SendMediaMessageToForeground(MediaMessageTypes.BackgroundAudioTaskStarted);
            settinghelper.Write(nameof(BackgroundTaskState), BackgroundTaskState.Running.ToString());

            deferral = taskInstance.GetDeferral(); // 这个必须比注册用到了它的event先进行

            // 将后台任务标记为已开始，释放SMTC的播放操作（见于此信号有关的WaitOne）
            backgroundTaskStarted.Set();

            // 关联任务取消和完成的handler
            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled); // event may raise immediately before continung thread excecution so must be at the end
        }

        /// <summary>
        /// 指示后台任务结束
        /// </summary>       
        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            deferral.Complete();
        }

        /// <summary>
        /// 处理后台任务的取消
        /// </summary>
        /// <remarks>
        /// 取消原因有
        /// 1.其他独占性的Media App运行到前台开始播放声音
        /// 2.系统资源不足
        /// </remarks>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            // 在此处可以在进程和资源被收回时保存状态
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                // 立即停止运行
                backgroundTaskStarted.Reset();

                // 保存现有状态
                settinghelper.Write(TrackIdKey, GetCurrentTrackId()?.ToString());
                settinghelper.Write(nameof(BackgroundMediaPlayer.Current.Position), BackgroundMediaPlayer.Current.Position.ToString());
                settinghelper.Write(nameof(BackgroundTaskState), BackgroundTaskState.Canceled.ToString());
                settinghelper.Write(nameof(AppState), Enum.GetName(typeof(AppState), foregroundAppState));

                // 取消列表更改事件
                if (playbackList != null)
                {
                    playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                    playbackList = null;
                }

                // 取消其他时间的Handler
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                smtc.ButtonPressed -= smtc_ButtonPressed;
                smtc.PropertyChanged -= smtc_PropertyChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            deferral.Complete(); //给出任务完成信号
            Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
        }
        #endregion

        #region SysteMediaTransportControls related functions and handlers
        /// <summary>
        /// 通过SystemMediaTransPortControl的API更新Universal Volume Control (UVC)
        /// </summary>
        private void UpdateUVCOnNewTrack(MediaPlaybackItem item)
        {
            if (item == null)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                smtc.DisplayUpdater.Update();
                return;
            }

            smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = item.Source.CustomProperties[TitleKey] as string;

            var albumArtUri = item.Source.CustomProperties[AlbumArtKey] as Uri;
            if (albumArtUri != null)
                smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
            else
                smtc.DisplayUpdater.Thumbnail = null;

            smtc.DisplayUpdater.Update();
        }

        private void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // 如果音量调至静音，应用可以选择暂停音乐
        }

        /// <summary>
        /// 处理UVC产生的按键事件
        /// </summary>
        /// <remarks>如果这段代码不运行在后台进程，则当其挂起时无法响应UVC事件</remarks>
        private void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");
                    
                    // 后台任务挂起后SMTC会异步启动，有时需要让在Run()中的启动过程完成

                    // 等待后台任务开始。一旦开始后，保持信号直到被关闭，使得不用再次等待，除非其他需要
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didnt initialize in time");
                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }

        #endregion

        #region Playlist management functions and handlers
        /// <summary>
        /// 开始播放列表并更改UVC状态
        /// </summary>
        private void StartPlayback()
        {
            try
            {
                // 如果播放已经开始过一次，则只需要继续播放
                if (!playbackStartedPreviously)
                {
                    playbackStartedPreviously = true;
                    // 如果任务被取消了，则从保存的音轨和位置开始播放
                    var currentTrackId = settinghelper.ReadAndReset<string>(TrackIdKey);
                    var currentTrackPosition = settinghelper.ReadAndReset<string>(nameof(BackgroundMediaPlayer.Current.Position));
                    if (currentTrackId != null)
                    {
                        // Find the index of the item by name
                        ExtensionMethods.ConsoleLog("No current track");
                        var index = playbackList.Items.ToList().FindIndex(item =>
                            GetTrackId(item).ToString() == currentTrackId);
                        if (currentTrackPosition == null)
                        {
                            // 如果没有保存过则从头开始播放
                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            playbackList.MoveTo((uint)index);
                            BackgroundMediaPlayer.Current.Play();
                        }
                        else
                        {
                            // 否则从保存的位置开始
                            TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
                            handler = (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
                            {
                                if (args.NewItem == playbackList.Items[index])
                                {
                                    // 删除订阅，因为对当前项只需要运行一次
                                    playbackList.CurrentItemChanged -= handler;
                                    
                                    var position = TimeSpan.Parse((string)currentTrackPosition);
                                    Debug.WriteLine("StartPlayback: Setting Position " + position);
                                    BackgroundMediaPlayer.Current.Position = position;
                                    
                                    BackgroundMediaPlayer.Current.Play();
                                }
                            };
                            playbackList.CurrentItemChanged += handler;
                            // 切换到当前音轨会触发ItemChanged事件
                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            playbackList.MoveTo((uint)index);
                        }
                    }
                    else
                        BackgroundMediaPlayer.Current.Play();
                }
                else
                    BackgroundMediaPlayer.Current.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        //当前播放的曲目改变时，即切换曲目时发生
        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            // 获取更新的项目
            var item = args.NewItem;
            Debug.WriteLine("PlaybackList_CurrentItemChanged: " + (item == null ? "null" : GetTrackId(item).ToString()));

            // 更新UVC
            UpdateUVCOnNewTrack(item);

            // 获取当前播放轨
            Uri currentTrackId = null;
            if (item != null)
                currentTrackId = item.Source.CustomProperties[TrackIdKey] as Uri;

            // 通知前台切换或者保持
            if (foregroundAppState == AppState.Active)
                MessageService.SendMediaMessageToForeground<Uri>(MediaMessageTypes.TrackChanged, currentTrackId);
            else
                settinghelper.Write(TrackIdKey, currentTrackId?.ToString());
        }

        private void SkipToPrevious()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MovePrevious();
        }

        private void SkipToNext()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MoveNext();
        }
        #endregion

        #region Background Media Player Handlers
        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            else if (sender.CurrentState == MediaPlayerState.Closed)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            ExtensionMethods.ConsoleLog($"Message {e.Data["MessageType"]} get");
            switch(MessageService.GetTypeOfMediaMessage(e.Data))
            {
                case MediaMessageTypes.AppSuspended:
                    Debug.WriteLine("App suspending"); // 应用被挂起，在此处保存应用状态
                    foregroundAppState = AppState.Suspended;
                    var currentTrackId = GetCurrentTrackId();
                    settinghelper.Write(TrackIdKey, currentTrackId?.ToString());
                    return;
                case MediaMessageTypes.AppResumed:
                    Debug.WriteLine("App resuming"); // 应用继续
                    foregroundAppState = AppState.Active;
                    return;
                case MediaMessageTypes.StartPlayback:
                    //应用前台发出播放信号
                    Debug.WriteLine("Starting Playback");
                    StartPlayback();
                    return;
                case MediaMessageTypes.SkipNext:
                    Debug.WriteLine("Skipping to next");
                    SkipToNext();
                    return;
                case MediaMessageTypes.SkipPrevious:
                    Debug.WriteLine("Skipping to previous");
                    SkipToPrevious();
                    return;
                case MediaMessageTypes.TrackChanged:
                    var index = playbackList.Items.ToList().FindIndex(i => (Uri)i.Source.CustomProperties[TrackIdKey] == MessageService.GetMediaMessage<Uri>(e.Data));
                    Debug.WriteLine("Skipping to track " + index);
                    smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                    playbackList.MoveTo((uint)index);
                    return;
                case MediaMessageTypes.UpdatePlaylist:
                    CreatePlaybackList(MessageService.GetMediaMessage<IEnumerable<SongModel>>(e.Data));
                    return;
            }

        }

        /// <summary>
        /// 为从前台任务收到的列表创建播放列表
        /// </summary>
        /// <param name="songs"></param>
        void CreatePlaybackList(IEnumerable<SongModel> songs)
        {
            // 生成新的列表并开启循环
            playbackList = new MediaPlaybackList();
            playbackList.AutoRepeatEnabled = true;
            foreach (var song in songs)
            {
                var source = MediaSource.CreateFromUri(song.MediaUri);
                source.CustomProperties[TrackIdKey] = song.MediaUri;
                source.CustomProperties[TitleKey] = song.Title;
                source.CustomProperties[AlbumArtKey] = song.AlbumArtUri;
                playbackList.Items.Add(new MediaPlaybackItem(source));
                ExtensionMethods.ConsoleLog($"song added length {source.Duration}");
            }
            BackgroundMediaPlayer.Current.AutoPlay = false;// 关闭自动播放
            BackgroundMediaPlayer.Current.Source = playbackList;
            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }
        #endregion
    }
}
