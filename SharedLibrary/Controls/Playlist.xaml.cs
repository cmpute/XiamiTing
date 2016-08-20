using System;
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

namespace JacobC.Xiami.Controls
{
    public sealed partial class Playlist : UserControl
    {
        public Playlist()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 获取或设置播放列表的类型
        /// </summary>
        public PlaylistType ListType
        {
            get { return (PlaylistType)GetValue(ListTypeProperty); }
            set { SetValue(ListTypeProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="ListType"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty ListTypeProperty =
              DependencyProperty.Register(nameof(ListType), typeof(PlaylistType),
                  typeof(Playlist), new PropertyMetadata(PlaylistType.LocalPlaylist, (d, e) =>
                  {
                      (d as Playlist).InternalListTypeChanged(e);
                  }));
        private void InternalListTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            switch((PlaylistType)(e.NewValue))
            {
                case PlaylistType.LocalPlaylist:
                    SelectionMode = ListViewSelectionMode.Multiple;
                    break;
            }
        }



        public object SongSource
        {
            get { return (object)GetValue(SongSourceProperty); }
            set { SetValue(SongSourceProperty, value); }
        } 
        public static readonly DependencyProperty SongSourceProperty =
            DependencyProperty.Register("SongSource", typeof(object), typeof(Playlist), new PropertyMetadata(null));



        /// <summary>
        /// 获取或设置列表的选择模式属性
        /// </summary>
        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="SelectionMode"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty =
              DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode),
                  typeof(Playlist), new PropertyMetadata(ListViewSelectionMode.Multiple, (d, e) =>
                  {
                      //(d as Playlist).SelectionModeChanged?.Invoke(d, e.ToChangedEventArgs<SelectionMode>());
                      (d as Playlist).InternalSelectionModeChanged(e);
                  }));
        /// <summary>
        /// 在<see cref="SelectionMode"/>属性发生变更时发生
        /// </summary>
        //public event EventHandler<ChangedEventArgs<ListViewSelectionMode>> SelectionModeChanged;
        private void InternalSelectionModeChanged(DependencyPropertyChangedEventArgs e)
        {

        }


    }

    public enum PlaylistType
    {
        AlbumPlaylist,
        CollectionPlaylist,
        LocalPlaylist
    }
}
