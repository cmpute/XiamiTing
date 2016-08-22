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

        private SongModel CurrentSong
        {
            get { return (SongModel)GetValue(CurrentSongProperty); }
            set { SetValue(CurrentSongProperty, value); }
        }
        public static readonly DependencyProperty CurrentSongProperty =
            DependencyProperty.Register("CurrentSong", typeof(SongModel), typeof(MusicController), new PropertyMetadata(SongModel.Null));

        /// <summary>
        /// 获取或设置是否播放内容为电台属性
        /// </summary>
        public bool IsPlayingRadio
        {
            get { return (bool)GetValue(IsPlayingRadioProperty); }
            private set { SetValue(IsPlayingRadioProperty, value); }
        }
        private static readonly bool _defaultIsPlayingRadio = default(bool);
        /// <summary>
        /// 标识<see cref="IsPlayingRadio"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty IsPlayingRadioProperty =
              DependencyProperty.Register(nameof(IsPlayingRadio), typeof(bool),
                  typeof(MusicController), new PropertyMetadata(_defaultIsPlayingRadio, (d, e) =>
                  {
                      (d as MusicController).IsPlayingRadioChanged?.Invoke(d, e.ToChangedEventArgs<bool>());
                      (d as MusicController).InternalIsPlayingRadioChanged(e.ToChangedEventArgs<bool>());
                  }));
        /// <summary>
        /// 在<see cref="IsPlayingRadio"/>属性发生变更时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<bool>> IsPlayingRadioChanged;
        partial void InternalIsPlayingRadioChanged(ChangedEventArgs<bool> e);


        #endregion

    }
}
