using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
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
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
                SongModel target = source.DataContext as SongModel;
                target.IsHovered = val > 1;
                target.IsSelected = (val % 2) != 0;
                System.Diagnostics.Debug.WriteLine($"Selected:{target.IsSelected}, Hover:{target.IsHovered}");
            });
        }
    }
}
