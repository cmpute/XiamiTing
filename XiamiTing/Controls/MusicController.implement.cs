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
    public sealed partial class MusicController : UserControl
    {
        #region Codes for Playback

        //private MainPage rootPage;
        private AutoResetEvent backgroundAudioTaskStarted = new AutoResetEvent(false);
        private Dictionary<string, BitmapImage> albumArtCache = new Dictionary<string, BitmapImage>();
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        DelegateCommand _PlayCommand;
        public DelegateCommand PlayCommand => _PlayCommand ?? (_PlayCommand = new DelegateCommand(() =>
            {
                Debug.WriteLine("Play button pressed from App");
                if (IsBackgroundTaskRunning)
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
                        StartBackgroundAudioTask();
                    }
                }
                else
                {
                    StartBackgroundAudioTask();
                }
            }));

        private bool _isBackgroundTaskRunning = false;
        private bool IsBackgroundTaskRunning
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
        private MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

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
        private void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    MessageService.SendMediaMessageToBackground(MediaMessageTypes.UpdatePlaylist, new List<SongModel>() { new SongModel() { Title = "testtitle", MediaUri = new Uri(@"http://win.web.rh03.sycdn.kuwo.cn/9cde1835bc61fe36d11291d29b43ae2e/5788f9ab/resource/a2/21/1/314466624.aac"),Album=new AlbumModel()} });
                    MessageService.SendMediaMessageToBackground(MediaMessageTypes.StartPlayback);
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            }).AsTask().ContinueWith((task) => {
                if (task.IsCompleted)
                {
                    Debug.WriteLine("Background Audio Task initialized");
                }
                else
                {
                    Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + task.Exception.ToString());
                }
            });
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
                    ExtensionMethods.ConsoleLog(ex.Message);
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
            if (MessageService.GetTypeOfMediaMessage(e.Data) == MediaMessageTypes.TrackChanged)
            {
                //When foreground app is active change track based on background message
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // If playback stopped then clear the UI
                    //string trackid = MessageService.GetMediaMessage<string>(e.Data);
                    //if (trackid == null)
                    //{
                    //    playlistView.SelectedIndex = -1;
                    //    albumArt.Source = null;
                    //    txtCurrentTrack.Text = string.Empty;
                    //    prevButton.IsEnabled = false;
                    //    nextButton.IsEnabled = false;
                    //    return;
                    //}

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
            }

            if (MessageService.GetTypeOfMediaMessage(e.Data) == MediaMessageTypes.BackgroundAudioTaskStarted)
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                Debug.WriteLine("BackgroundAudioTask started");
                backgroundAudioTaskStarted.Set();
                return;
            }
        }
        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
            {
                //// Update state label
                //txtCurrentState.Text = currentState.ToString();

                //// Update controls
                //UpdateTransportControls(currentState);
            });
        }

        #endregion
    }
}
