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

namespace JacobC.Xiami.Controls
{
    public sealed partial class MusicController : UserControl
    {

        #region Binding Properties

        /// <summary>
        /// 获取或设置播放界面的专辑封面
        /// </summary>
        public ImageSource AlbumArt
        {
            get { return GetValue(AlbumArtProperty) as ImageSource; }
            set { SetValue(AlbumArtProperty, value); }
        }
        private static readonly ImageSource _defaultAlbumArt = new BitmapImage(new Uri(@"ms-appx:///Assets/Pictures/cd100.gif"));
        public static readonly DependencyProperty AlbumArtProperty =
              DependencyProperty.Register(nameof(AlbumArt), typeof(ImageSource),
                  typeof(MusicController), new PropertyMetadata(_defaultAlbumArt, (d, e) =>
                  {
                      (d as MusicController).AlbumArtChanged?.Invoke(d, e.ToChangedEventArgs<ImageSource>());
                      (d as MusicController).InternalAlbumArtChanged(e.ToChangedEventArgs<ImageSource>());
                  }));
        /// <summary>
        /// 在<see cref="AlbumArt"/>属性发生变更时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<ImageSource>> AlbumArtChanged;
        partial void InternalAlbumArtChanged(ChangedEventArgs<ImageSource> e);


        #endregion

    }
}
