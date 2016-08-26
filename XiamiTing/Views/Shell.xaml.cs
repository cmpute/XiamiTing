using System.ComponentModel;
using System.Linq;
using Template10.Common;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JacobC.Xiami.Views
{
    public sealed partial class Shell : Page
    {
        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu => Instance.RootFrame;

        public Shell()
        {
            Instance = this;
            InitializeComponent();

            JacobC.Xiami.Services.SettingsService.Instance.AppThemeChanged += (value) =>
                  HamburgerMenu.RefreshStyles(value);
        }

        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        public void SetNavigationService(INavigationService navigationService)
        {
            RootFrame.NavigationService = navigationService;
        }
    }
}

