using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace JacobC.Xiami.ViewModels
{
    public class PlaylistPageViewModel : BindableBase
    {
        public ObservableCollection<SongModel> Playlist
        {
            get { return PlaylistService.Playlist; }
        }
    }
}
