using JacobC.Xiami.Services;
using JacobC.Xiami.Net;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace JacobC.Xiami
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : BootStrapper
    {
        public App()
        {
            InitializeComponent();
            //SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var _settings = SettingsService.Instance;
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            #endregion
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            //自定义代码段
            SetTitleColor();
            LogService.DebugWrite(PlaylistService.Instance.Playlist.Count.ToString(), "Starting"); // Initialize Playlist
            PlaybackService.Instance.StartBackgroundAudioTask();

            if (Window.Current.Content as ModalDialog == null)
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);

                // create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(nav),
                    ModalContent = new Views.Busy(),
                };
            }
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // long-running startup tasks go here
            //await Task.Delay(5000);

            NavigationService.Navigate(typeof(Views.MainPage));
            await Task.CompletedTask;
        }

        /// <summary>
        /// 设置窗口标题栏的样式
        /// <!--此段代码为自定义-->
        /// </summary>
        private void SetTitleColor()
        {
            var titlebar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            Color theme = (Color)(Resources["ThemeColor"]);
            Color inactive = (Color)(Resources["InactiveThemeColor"]);
            Color hover = (Color)(Resources["HighlightThemeColor"]);
            //活动色
            titlebar.BackgroundColor = theme;
            titlebar.ForegroundColor = Colors.White;
            titlebar.ButtonBackgroundColor = theme;
            titlebar.ButtonForegroundColor = Colors.White;
            //非活动色
            titlebar.InactiveBackgroundColor = inactive;
            titlebar.InactiveForegroundColor = Colors.White;
            titlebar.ButtonInactiveBackgroundColor = inactive;
            titlebar.ButtonInactiveForegroundColor = Colors.White;
            //事件色
            titlebar.ButtonHoverBackgroundColor = hover;
            titlebar.ButtonHoverForegroundColor = Colors.White;
            titlebar.ButtonPressedBackgroundColor = inactive;
        }
    }
}
