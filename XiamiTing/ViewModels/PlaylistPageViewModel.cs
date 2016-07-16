using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using JacobC.Xiami.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;

namespace JacobC.Xiami.ViewModels
{
    public class PlaylistPageViewModel : ViewModelBase
    {
        public PlaylistPageViewModel()
        {
            PlaylistService.Instance.CurrentIndexChanging += (sender, e) => this.Set(ref _CurrentPlayingIndex, e.NewValue, nameof(CurrentPlayingIndex));
        }

        public ObservableCollection<SongModel> Playlist
        {
            get { return PlaylistService.Instance.Playlist; }
        }

        int _CurrentPlayingIndex = default(int);
        /// <summary>
        /// 获取或设置当前播放的音轨属性
        /// </summary>
        public int CurrentPlayingIndex { get { return PlaylistService.Instance.CurrentIndex; } set { PlaylistService.Instance.CurrentIndex = value; } }

    }
}
