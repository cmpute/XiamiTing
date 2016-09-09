using System;
using Template10.Common;
using Template10.Utils;
using Template10.Services.SettingsService;
using Windows.UI.Xaml;

namespace JacobC.Xiami.Services
{
    public class SettingsService
    {
        public static SettingsService Instance { get; } = new SettingsService();
        private SettingsService() { }

        /// <summary>
        /// 普通设置
        /// </summary>
        public static ISettingsService General
        {
            get { return Template10.Services.SettingsService.SettingsService.Local; }
        }

        /// <summary>
        /// 播放相关设置
        /// </summary>
        public static ISettingsService Playback { get; } = Template10.Services.SettingsService.SettingsService.Local.Open("Playback");

        /// <summary>
        /// Cookie相关设置
        /// </summary>
        public static ISettingsService NetCookies { get; } = Template10.Services.SettingsService.SettingsService.Local.Open("NetCookies");

        /// <summary>
        /// 帐户相关设置
        /// </summary>
        public static ISettingsService Account { get; } = Template10.Services.SettingsService.SettingsService.Local.Open("Account");

        /// <summary>
        /// 是否在标题栏显示返回按钮
        /// </summary>
        public bool UseShellBackButton
        {
            get { return General.Read<bool>(nameof(UseShellBackButton), true); }
            set
            {
                General.Write(nameof(UseShellBackButton), value);
                BootStrapper.Current.NavigationService.Dispatcher.Dispatch(() =>
                {
                    BootStrapper.Current.ShowShellBackButton = value;
                    BootStrapper.Current.UpdateShellBackButton();
                    BootStrapper.Current.NavigationService.Refresh();
                });
            }
        }

        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Light;
                var value = General.Read<string>(nameof(AppTheme), theme.ToString());
                return Enum.TryParse<ApplicationTheme>(value, out theme) ? theme : ApplicationTheme.Dark;
            }
            set
            {
                General.Write(nameof(AppTheme), value.ToString());
                (Window.Current.Content as FrameworkElement).RequestedTheme = value.ToElementTheme();
                AppThemeChanged?.Invoke(value);
            }
        }
        public event Action<ApplicationTheme> AppThemeChanged;

        public TimeSpan CacheMaxDuration
        {
            get { return General.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                General.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

    }
}

