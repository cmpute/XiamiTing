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

namespace JacobC.Xiami.Controls
{
    public sealed partial class MusicController : UserControl
    {

        #region Binding Properties

        /// <summary>
        /// 获取或设置当前播放的歌曲
        /// </summary>
        public SongModel CurrentSong
        {
            get { return GetValue(CurrentSongProperty) as SongModel; }
            private set { SetValue(CurrentSongProperty, value); }
        }
        private static readonly SongModel _defaultCurrentSong = new SongModel { Album = new AlbumModel() };
        /// <summary>
        /// 标识<see cref="CurrentSong"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty CurrentSongProperty =
              DependencyProperty.Register(nameof(CurrentSong), typeof(SongModel),
                  typeof(MusicController), new PropertyMetadata(_defaultCurrentSong, (d, e) =>
                  {
                      (d as MusicController).CurrentSongChanged?.Invoke(d, e.ToChangedEventArgs<SongModel>());
                      (d as MusicController).InternalCurrentSongChanged(e.ToChangedEventArgs<SongModel>());
                  }));
        /// <summary>
        /// 在<see cref="CurrentSong"/>属性发生变更时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<SongModel>> CurrentSongChanged;
        partial void InternalCurrentSongChanged(ChangedEventArgs<SongModel> e);

        #endregion

    }
}
