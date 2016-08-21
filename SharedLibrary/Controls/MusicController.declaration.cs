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
            DependencyProperty.Register("CurrentSong", typeof(SongModel), typeof(MusicController), new PropertyMetadata(null));


        #endregion

    }
}
