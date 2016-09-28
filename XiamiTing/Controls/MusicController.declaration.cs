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

        public SongModel CurrentSong
        {
            get { return (SongModel)GetValue(CurrentSongProperty); }
            private set { SetValue(CurrentSongProperty, value); }
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
        /// <summary>
        /// 标识<see cref="IsPlayingRadio"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty IsPlayingRadioProperty =
              DependencyProperty.Register(nameof(IsPlayingRadio), typeof(bool),
                  typeof(MusicController), new PropertyMetadata(false, (d, e) =>
                  {
                      //(d as MusicController).IsPlayingRadioChanged?.Invoke(d, e.ToChangedEventArgs<bool>());
                      (d as MusicController).InternalIsPlayingRadioChanged(e.ToChangedEventArgs<bool>());
                  }));
        partial void InternalIsPlayingRadioChanged(ChangedEventArgs<bool> e);


        /// <summary>
        /// 获取或设置是否禁用下载ProgressBar属性
        /// </summary>
        /// <remarks>
        /// TODO: 网速太快，测不出有buffer的过程
        /// </remarks>
        public bool IsDownloadBarDisabled
        {
            get { return (bool)GetValue(IsDownloadBarDisabledProperty); }
            set { SetValue(IsDownloadBarDisabledProperty, value); }// TODO: 更改这个属性的UI线程不对，操作产生Exception
        }
        /// <summary>
        /// 标识<see cref="IsDownloadBarDisabled"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty IsDownloadBarDisabledProperty =
              DependencyProperty.Register(nameof(IsDownloadBarDisabled), typeof(bool),
                  typeof(MusicController), new PropertyMetadata(true));


        /// <summary>
        /// 获取或设置拖拽滑动条的时候是否实时改变音频位置
        /// </summary>
        public bool IsSeekingInstant { get; set; } = false;


        /// <summary>
        /// 获取或设置是否在滑动时显示时间提示框
        /// </summary>
        public bool IsThumbToolTipEnabled
        {
            get { return (bool)GetValue(IsThumbToolTipEnabledProperty); }
            set { SetValue(IsThumbToolTipEnabledProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="IsThumbToolTipEnabled"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty IsThumbToolTipEnabledProperty =
            DependencyProperty.Register("IsThumbToolTipEnabled", typeof(bool), typeof(MusicController), new PropertyMetadata(true));


        #endregion

    }
}
