using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using JacobC.Xiami.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Mvvm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data ;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using JacobC.Xiami.ViewModels;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace JacobC.Xiami.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistPage()
        {
            this.InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid source = sender as Grid;
            source.RegisterPropertyChangedCallback(Grid.TagProperty, (dpsender, dp) => 
            {
                int val = (int)(dpsender.GetValue(dp) ?? 0);
                SongViewModel target = source.DataContext as SongViewModel;
                target.IsHovered = val > 1;
                target.IsSelected = (val % 2) != 0;
            });
        }

        private DelegateCommand<object> _DeleteCommand;
        public DelegateCommand<object> DeleteCommand => _DeleteCommand ?? (_DeleteCommand = new DelegateCommand<object>((model) =>
        {
            var Playlist = PlaylistService.Instance.Playlist;
            for (int i = Playlist.Count - 1; i >= 0; i--)
                if (Playlist[i].IsSelected)
                    Playlist.RemoveAt(i);
        }, (model) => { return listView.SelectedItems.Count != 0; }));


        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _DeleteCommand.RaiseCanExecuteChanged();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //System.Diagnostics.Debugger.Break();
            PlaylistService.Instance.Playlist.Remove(((Button)sender).DataContext as SongViewModel);
            //判断是否在播放
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var target = ((Button)sender).DataContext as SongViewModel;
            PlaybackService.Instance.PlayTrack(target.Model);
            PlaylistService.Instance.CurrentPlaying = target;
        }
    }
}
