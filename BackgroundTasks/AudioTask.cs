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

            deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
            backgroundTaskStarted.Set();

            // Associate a cancellation and completed handlers with the background task.
            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled); // event may raise immediately before continung thread excecution so must be at the end
        }

        /// <summary>
        /// 指示后台任务结束
        /// </summary>       
        void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            deferral.Complete();
        }

        /// <summary>
        /// 处理后台任务的取消
        /// </summary>
        /// <remarks>
        /// 取消原因有
        /// 1.其他Media App运行到前台开始播放音乐
        /// 2.系统资源不足
        /// </remarks>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
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

        void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // If soundlevel turns to muted, app can choose to pause the music
        }

        /// <summary>
        /// 处理UVC产生的按键事件
        /// This function controls the button events from UVC.
        /// This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>如果这段代码不运行在后台进程，则当其挂起时无法响应UVC事件</remarks>
        private void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");

                    // When the background task has been suspended and the SMTC
                    // starts it again asynchronously, some time is needed to let
                    // the task startup process in Run() complete.

                    // Wait for task to start. 
                    // Once started, this stays signaled until shutdown so it won't wait
                    // again unless it needs to.
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
                // If playback was already started once we can just resume playing.
                if (!playbackStartedPreviously)
                {
                    playbackStartedPreviously = true;

                    // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
                    var currentTrackId = settinghelper.ReadAndReset<string>(TrackIdKey);
                    var currentTrackPosition = settinghelper.ReadAndReset<string>(nameof(BackgroundMediaPlayer.Current.Position));
                    if (currentTrackId != null)
                    {
                        // Find the index of the item by name
                        var index = playbackList.Items.ToList().FindIndex(item =>
                            GetTrackId(item).ToString() == (string)currentTrackId);

                        if (currentTrackPosition == null)
                        {
                            // Play from start if we dont have position
                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            playbackList.MoveTo((uint)index);

                            // Begin playing
                            BackgroundMediaPlayer.Current.Play();
                        }
                        else
                        {
                            // Play from exact position otherwise
                            TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
                            handler = (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
                            {
                                if (args.NewItem == playbackList.Items[index])
                                {
                                    // Unsubscribe because this only had to run once for this item
                                    playbackList.CurrentItemChanged -= handler;

                                    // Set position
                                    var position = TimeSpan.Parse((string)currentTrackPosition);
                                    Debug.WriteLine("StartPlayback: Setting Position " + position);
                                    BackgroundMediaPlayer.Current.Position = position;

                                    // Begin playing
                                    BackgroundMediaPlayer.Current.Play();
                                }
                            };
                            playbackList.CurrentItemChanged += handler;

                            // Switch to the track which will trigger an item changed event
                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            playbackList.MoveTo((uint)index);
                        }
                    }
                    else
                    {
                        // Begin playing
                        BackgroundMediaPlayer.Current.Play();
                    }
                }
                else
                {
                    // Begin playing
                    BackgroundMediaPlayer.Current.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Raised when playlist changes to a new track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            // Get the new item
            var item = args.NewItem;
            Debug.WriteLine("PlaybackList_CurrentItemChanged: " + (item == null ? "null" : GetTrackId(item).ToString()));

            // Update the system view
            UpdateUVCOnNewTrack(item);

            // Get the current track
            Uri currentTrackId = null;
            if (item != null)
                currentTrackId = item.Source.CustomProperties[TrackIdKey] as Uri;

            // Notify foreground of change or persist for later
            if (foregroundAppState == AppState.Active)
                MessageService.SendMediaMessageToForeground<Uri>(MediaMessageTypes.TrackChanged, currentTrackId);
            else
                settinghelper.Write(TrackIdKey, currentTrackId?.ToString());
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToPrevious()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MovePrevious();
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
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

        /// <summary>
        /// Raised when a message is recieved from the foreground app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            switch(MessageService.GetTypeOfMessage(e.Data))
            {
                case MediaMessageTypes.AppSuspended:
                    Debug.WriteLine("App suspending"); // App is suspended, you can save your task state at this point
                    foregroundAppState = AppState.Suspended;
                    var currentTrackId = GetCurrentTrackId();
                    settinghelper.Write(TrackIdKey, currentTrackId?.ToString());
                    return;
                case MediaMessageTypes.AppResumed:
                    Debug.WriteLine("App resuming"); // App is resumed, now subscribe to message channel
                    foregroundAppState = AppState.Active;
                    return;
                case MediaMessageTypes.StartPlayback:
                    //Foreground App process has signalled that it is ready for playback
                    Debug.WriteLine("Starting Playback");
                    StartPlayback();
                    return;
                case MediaMessageTypes.SkipNext:
                    // User has chosen to skip track from app context.
                    Debug.WriteLine("Skipping to next");
                    SkipToNext();
                    return;
                case MediaMessageTypes.SkipPrevious:
                    // User has chosen to skip track from app context.
                    Debug.WriteLine("Skipping to previous");
                    SkipToPrevious();
                    return;
                case MediaMessageTypes.TrackChanged:
                    var index = playbackList.Items.ToList().FindIndex(i => (Uri)i.Source.CustomProperties[TrackIdKey] == MessageService.GetMessage<Uri>(e.Data));
                    Debug.WriteLine("Skipping to track " + index);
                    smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                    playbackList.MoveTo((uint)index);
                    return;
                case MediaMessageTypes.UpdatePlaylist:
                    CreatePlaybackList(MessageService.GetMessage<IEnumerable<SongModel>>(e.Data));
                    return;
            }

        }

        /// <summary>
        /// Create a playback list from the list of songs received from the foreground app.
        /// </summary>
        /// <param name="songs"></param>
        void CreatePlaybackList(IEnumerable<SongModel> songs)
        {
            // Make a new list and enable looping
            playbackList = new MediaPlaybackList();
            playbackList.AutoRepeatEnabled = true;

            // Add playback items to the list
            foreach (var song in songs)
            {
                var source = MediaSource.CreateFromUri(song.MediaUri);
                source.CustomProperties[TrackIdKey] = song.MediaUri;
                source.CustomProperties[TitleKey] = song.Title;
                source.CustomProperties[AlbumArtKey] = song.AlbumArtUri;
                playbackList.Items.Add(new MediaPlaybackItem(source));
            }

            // Don't auto start
            BackgroundMediaPlayer.Current.AutoPlay = false;

            // Assign the list to the player
            BackgroundMediaPlayer.Current.Source = playbackList;

            // Add handler for future playlist item changes
            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }
        #endregion
    }

    public enum AppState
    {
        Unknown,
        Active,
        Suspended
    }
    public enum BackgroundTaskState
    {
        Unknown,
        Started,
        Running,
        Canceled
    }
}
