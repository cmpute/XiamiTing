using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using JacobC.Xiami.ViewModels;
using Template10.Services.NavigationService;
using Template10.Utils;

namespace JacobC.Xiami.Views
{
    public sealed partial class DiscoveryPage : Page
    {
        public DiscoveryPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void AlbumItem_MainPanelClick(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DiscoveryViewModel).NavigateAlbum(sender as Models.AlbumModel);
        }
    }
}
