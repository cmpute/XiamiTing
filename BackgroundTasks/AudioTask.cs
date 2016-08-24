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
using static JacobC.Xiami.Services.LogService;

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
        private BackgroundTaskDeferral deferral; // 保持任务活动
        private AppState foregroundAppState = AppState.Unknown;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private bool playbackStartedPreviously = false;
        Template10.Services.SettingsService.ISettingsService settinghelper = SettingsService.Instance.Playback;
        #endregion

        #region Helper methods

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
            DebugWrite("Background Audio Task " + taskInstance.Task.Name + " starting...", "BackgroundPlayer");

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
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged; //为播放器添加后台控制句柄
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground; //初始化消息通道

            foregroundAppState = settinghelper.ReadAndReset(nameof(AppState), AppState.Unknown); //读取APP状态
            //if (foregroundAppState != AppState.Suspended)
            //    settinghelper.Write("BackgroundAudioStarted", true);
            settinghelper.Write(nameof(BackgroundTaskState), BackgroundTaskState.Running.ToString());

            deferral = taskInstance.GetDeferral(); // 这个必须比注册用到了它的event先进行

            // 将后台任务标记为已开始，释放SMTC的播放操作（见于此信号有关的WaitOne）
            backgroundTaskStarted.Set();

            // 关联任务取消和完成的handler
            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += OnCanceled;
        }
        /// <summary>
        /// 指示后台任务结束
        /// </summary>       
        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            DebugWrite("BackgroundAudioTask " + sender.TaskId + " Completed...", "BackgroundPlayer");
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
            // 在此处可以在进程和资源被收回时保存状态
            DebugWrite("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...", "BackgroundPlayer");
            try
            {
                // 立即停止运行
                backgroundTaskStarted.Reset();

                // 保存现有状态
                //settinghelper.Write(TrackIdKey, GetCurrentTrackId()?.ToString());
                settinghelper.Write(nameof(BackgroundMediaPlayer.Current.Position), BackgroundMediaPlayer.Current.Position.ToString());
                settinghelper.Write(nameof(BackgroundTaskState), BackgroundTaskState.Canceled.ToString());
                settinghelper.Write(nameof(AppState), Enum.GetName(typeof(AppState), foregroundAppState));

                // 取消其他事件的Handler
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                smtc.ButtonPressed -= smtc_ButtonPressed;
                smtc.PropertyChanged -= smtc_PropertyChanged;
            }
            catch (Exception ex)
            {
                ErrorWrite(ex, "BackgroundPlayer");
            }
            //settinghelper.Remove("BackgroundAudioStarted");
            deferral.Complete(); //给出任务完成信号
            DebugWrite("BackgroundAudioTask Cancel complete...", "BackgroundPlayer");
        }
        #endregion

        #region SysteMediaTransportControls related functions and handlers
        /// <summary>
        /// 通过<see cref="SystemMediaTransportControls"/>的API更新Universal Volume Control (UVC)
        /// </summary>
        private void UpdateUVCOnNewTrack(SongModel item)
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
            smtc.DisplayUpdater.MusicProperties.Title = item.Name;
            smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(item.Album.Art);
            smtc.DisplayUpdater.Update();
        }

        private void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // TODO: 如果音量调至静音，应用可以选择暂停音乐
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
                    DebugWrite("UVC play button pressed", "BackgroundPlayer");
                    
                    // 后台任务挂起后SMTC会异步启动，有时需要让在Run()中的启动过程完成

                    // 等待后台任务开始。一旦开始后，保持信号直到被关闭，使得不用再次等待，除非其他需要
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didn't initialize in time");
                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    DebugWrite("UVC pause button pressed", "BackgroundPlayer");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        ErrorWrite(ex, "BackgroundPlayer");
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    DebugWrite("UVC next button pressed", "BackgroundPlayer");
                    MessageService.SendMediaMessageToForeground(MediaMessageTypes.SkipNext);
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    DebugWrite("UVC previous button pressed", "BackgroundPlayer");
                    MessageService.SendMediaMessageToForeground(MediaMessageTypes.SkipPrevious);
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
                DebugWrite("into Start method", "BackgroundPlayer");
                // 如果播放已经开始过一次，则只需要继续播放
                if (!playbackStartedPreviously)
                {
                    playbackStartedPreviously = true;
                    var currentTrackId = settinghelper.ReadAndReset<string>(TrackIdKey);
                    var currentTrackPosition = settinghelper.ReadAndReset<string>(nameof(BackgroundMediaPlayer.Current.Position));
                    if (currentTrackPosition != null)
                    {
                        // 如果任务被取消了，则从保存的音轨和位置开始播放
                        BackgroundMediaPlayer.Current.Play();
                    }
                    else
                    {
                        DebugWrite("No current track", "BackgroundPlayer");
                        BackgroundMediaPlayer.Current.Play();
                    }
                }
                else
                {
                    DebugWrite("started previously", "BackgroundPlayer");
                    BackgroundMediaPlayer.Current.Play();
                }
            }
            catch (Exception ex)
            {
                ErrorWrite(ex, "BackgroundPlayer");
            }
        }

        //当前播放的曲目改变时，即切换曲目时发生
        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            // 获取更新的项目
            var item = args.NewItem;
            DebugWrite("PlaybackList_CurrentItemChanged: " + (item == null ? "null" : GetTrackId(item).ToString()), "BackgroundPlayer");
            //System.Diagnostics.Debugger.Break();
            //if (item == null)
            //    UpdateUVCOnNewTrack(null);

            //// 获取当前播放轨
            //Uri currentTrackId = null;
            //if (item != null)
            //    currentTrackId = item.Source.CustomProperties[TrackIdKey] as Uri;

            //// 通知前台切换或者保持
            //if (foregroundAppState == AppState.Active)
            //    MessageService.SendMediaMessageToForeground<Uri>(MediaMessageTypes.TrackChanged, currentTrackId);
            //else
            //    settinghelper.Write(TrackIdKey, currentTrackId?.ToString());
        }

        #endregion

        #region Background Media Player Handlers
        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            DebugWrite($"PlayerStateChanged to {sender.CurrentState.ToString()}", "BackgroundPlayer");
            switch(sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused://中途的暂停或播放完毕的暂停
                    smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Closed:
                    smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaPlayerState.Opening://打开文件
                    break;
            }
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            //System.Diagnostics.Debugger.Break();
            DebugWrite($"Message {e.Data["MessageType"]} get", "BackgroundPlayer");
            switch(MessageService.GetTypeOfMediaMessage(e.Data))
            {
                case MediaMessageTypes.AppSuspended:
                    DebugWrite("App suspending", "BackgroundPlayer"); // 应用被挂起，在此处保存应用状态
                    foregroundAppState = AppState.Suspended;
                    //var currentTrackId = GetCurrentTrackId();
                    //settinghelper.Write(TrackIdKey, currentTrackId?.ToString());
                    return;
                case MediaMessageTypes.AppResumed:
                    DebugWrite("App resuming", "BackgroundPlayer"); // 应用继续
                    foregroundAppState = AppState.Active;
                    return;
                case MediaMessageTypes.StartPlayback:
                    //应用前台发出播放信号
                    DebugWrite("Starting Playback", "BackgroundPlayer");
                    StartPlayback();
                    return;
                case MediaMessageTypes.SetSong:
                    var song = MessageService.GetMediaMessage<SongModel>(e.Data);
                    smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                    var current = BackgroundMediaPlayer.Current;
                    current.SetUriSource(song.MediaUri);
                    //if (current.CurrentState != MediaPlayerState.Playing && current.CurrentState != MediaPlayerState.Closed)
                    //    StartPlayback();
                    UpdateUVCOnNewTrack(song);
                    DebugWrite($"PlaySong {song.XiamiID} Address:{song.MediaUri}", "BackgroundPlayer");
                    return;
            }

        }

        private void PlaybackList_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
        {
            DebugWrite(args.Error.ErrorCode.ToString(), "BackgroundPlayer ItemFailed");
        }
        #endregion
    }
}
