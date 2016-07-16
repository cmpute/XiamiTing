using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using JacobC.Xiami.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace JacobC.Xiami.ViewModels
{
    public class PlaylistPageViewModel : ViewModelBase
    {
        public PlaylistPageViewModel()
        {
            PlaylistService.Instance.CurrentIndexChanging += (sender, e) => this.Set(ref _CurrentIndex, e.NewValue, nameof(CurrentIndex));
        }

        public ObservableCollection<SongModel> Playlist
        {
            get { return PlaylistService.Instance.Playlist; }
        }

        int _CurrentIndex = default(int);
        /// <summary>
        /// 获取或设置当前选中的音轨属性
        /// </summary>
        public int CurrentIndex { get { return PlaylistService.Instance.CurrentIndex; } set { PlaylistService.Instance.CurrentIndex = value; } }

    }
}
