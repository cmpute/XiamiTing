﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        public void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void VM_SearchingFinished(object sender, Models.SearchResult e)
        {
            SongResults.ItemsSource = e.Songs;
            AlbumResults.ItemsSource = e.Albums;
            ArtistResults.ItemsSource = e.Artists;
            ResultList.Visibility = Visibility.Visible;
        }
    }
}
