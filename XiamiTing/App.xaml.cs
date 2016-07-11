using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Template10.Controls;
using Template10.Common;
using System.Threading.Tasks;
using JacobC.Xiami.Services;
using Windows.UI;

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
            SetTitleColor();
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
