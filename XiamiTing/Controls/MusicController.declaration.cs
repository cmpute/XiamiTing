using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using JacobC.Xiami.Models;
using JacobC.Xiami.ViewModels;

namespace JacobC.Xiami.Controls
{
    public sealed partial class MusicController : UserControl
    {

        #region Binding Properties

        /// <summary>
        /// 获取或设置当前播放的歌曲
        /// </summary>
        public SongViewModel CurrentSong
        {
            get { return GetValue(CurrentSongProperty) as SongViewModel; }
            private set { SetValue(CurrentSongProperty, value); }
        }
        private static readonly SongViewModel _defaultCurrentSong = null;
        /// <summary>
        /// 标识<see cref="CurrentSong"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty CurrentSongProperty =
              DependencyProperty.Register(nameof(CurrentSong), typeof(SongViewModel),
                  typeof(MusicController), new PropertyMetadata(_defaultCurrentSong, (d, e) =>
                  {
                      (d as MusicController).CurrentSongChanged?.Invoke(d, e.ToChangedEventArgs<SongViewModel>());
                      (d as MusicController).InternalCurrentSongChanged(e.ToChangedEventArgs<SongViewModel>());
                  }));
        /// <summary>
        /// 在<see cref="CurrentSong"/>属性发生变更时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<SongViewModel>> CurrentSongChanged;
        partial void InternalCurrentSongChanged(ChangedEventArgs<SongViewModel> e);

        //TODO: !!IsNowPlaying目前绑定失效
        /// <summary>
        /// 获取是否正在播放属性
        /// </summary>
        public bool IsNowPlaying
        {
            get { return (bool)GetValue(IsNowPlayingProperty); }
            private set { SetValue(IsNowPlayingProperty, value); }
        }
        private static readonly bool _defaultIsPlaying = default(bool);
        /// <summary>
        /// 标识<see cref="IsNowPlaying"/>依赖属性
        /// </summary>
        private static readonly DependencyProperty IsNowPlayingProperty =
              DependencyProperty.Register(nameof(IsNowPlaying), typeof(bool),
                  typeof(MusicController), new PropertyMetadata(_defaultIsPlaying, (d, e) => { }));


        #endregion

    }
}
