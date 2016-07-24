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
        SettingsHelper _helper;
        private SettingsService()
        {
            _helper = new SettingsHelper();
            _CacheItemsInDict = _helper.Read(nameof(CacheItemsInDict), false);
        }

        public SettingsHelper Helper
        {
            get { return _helper; }
        }

        /// <summary>
        /// �Ƿ��ڱ�������ʾ���ذ�ť
        /// </summary>
        public bool UseShellBackButton
        {
            get { return _helper.Read<bool>(nameof(UseShellBackButton), true); }
            set
            {
                _helper.Write(nameof(UseShellBackButton), value);
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
                var value = _helper.Read<string>(nameof(AppTheme), theme.ToString());
                return Enum.TryParse<ApplicationTheme>(value, out theme) ? theme : ApplicationTheme.Dark;
            }
            set
            {
                _helper.Write(nameof(AppTheme), value.ToString());
                (Window.Current.Content as FrameworkElement).RequestedTheme = value.ToElementTheme();
                AppThemeChanged?.Invoke(value);
            }
        }
        public event Action<ApplicationTheme> AppThemeChanged;

        public TimeSpan CacheMaxDuration
        {
            get { return _helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

        bool _CacheItemsInDict;
        /// <summary>
        /// �Ƿ���<see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>�洢����ר������Ϣ�����û���ټ���ʱ�䵫������ڴ�ռ��
        /// </summary>
        public bool CacheItemsInDict
        {
            get { return _CacheItemsInDict; }
            set
            {
                if (_CacheItemsInDict != value)
                {
                    _CacheItemsInDict = value;
                    _helper.Write(nameof(CacheItemsInDict), value);
                }
            }
        }
    }
}

