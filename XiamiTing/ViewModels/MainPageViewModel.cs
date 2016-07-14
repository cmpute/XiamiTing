using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;
using JacobC.Xiami.Services;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading;
using Windows.Foundation;
using JacobC.Xiami.Models;

namespace JacobC.Xiami.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }
        }

        string _Value = "Gas";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                Value = suspensionState[nameof(Value)]?.ToString();
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

        /***********************播放功能的代码*************************/

        //private MainPage rootPage;
        private AutoResetEvent backgroundAudioTaskStarted = new AutoResetEvent(false);
        private bool _isMyBackgroundTaskRunning = false;
        private Dictionary<string, BitmapImage> albumArtCache = new Dictionary<string, BitmapImage>();
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        DelegateCommand _PlayCommand;
        public DelegateCommand PlayCommand
            => _PlayCommand ?? (_PlayCommand = new DelegateCommand(() =>
            {
                Debug.WriteLine("Play button pressed from App");
                if (IsMyBackgroundTaskRunning)
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
        private bool IsMyBackgroundTaskRunning
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
            _isMyBackgroundTaskRunning = false;
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

            this.Dispatcher.DispatchAsync(() =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    MessageService.SendMediaMessageToBackground(MediaMessageTypes.UpdatePlaylist, new List<SongModel>() { new SongModel() {Name = "test", Title="testtitle", MediaUri=new Uri(@"http://win.web.rb03.sycdn.kuwo.cn/8f7092393283190ebc7033197eb07311/5787123c/resource/a3/73/65/3736166827.aac")  } });
                    MessageService.SendMediaMessageToBackground(MediaMessageTypes.StartPlayback);
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            }).ContinueWith((task) => {
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
            if (MessageService.GetTypeOfMediaMessage(e.Data)==MediaMessageTypes.TrackChanged)
            {
                // When foreground app is active change track based on background message
                //await this.Dispatcher.Dispatch(() =>
                //{
                //    // If playback stopped then clear the UI
                //    if (trackChangedMessage.TrackId == null)
                //    {
                //        playlistView.SelectedIndex = -1;
                //        albumArt.Source = null;
                //        txtCurrentTrack.Text = string.Empty;
                //        prevButton.IsEnabled = false;
                //        nextButton.IsEnabled = false;
                //        return;
                //    }

                //    var songIndex = playlistView.GetSongIndexById(trackChangedMessage.TrackId);
                //    var song = playlistView.Songs[songIndex];

                //    // Update list UI
                //    playlistView.SelectedIndex = songIndex;

                //    // Update the album art
                //    albumArt.Source = albumArtCache[song.AlbumArtUri.ToString()];

                //    // Update song title
                //    txtCurrentTrack.Text = song.Title;

                //    // Ensure track buttons are re-enabled since they are disabled when pressed
                //    prevButton.IsEnabled = true;
                //    nextButton.IsEnabled = true;
                //});
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
        async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            //var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            //await this.Dispatcher.Dispatch(() =>
            //{
            //    // Update state label
            //    txtCurrentState.Text = currentState.ToString();

            //    // Update controls
            //    UpdateTransportControls(currentState);
            //});
        }
    }
}

