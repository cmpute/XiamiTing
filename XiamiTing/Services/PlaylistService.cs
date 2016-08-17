using JacobC.Xiami.Models;
using static JacobC.Xiami.Services.LogService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Utils;
using System.Threading;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using JacobC.Xiami.ViewModels;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 维护全局的播放列表集合
    /// </summary>
    public class PlaylistService
    {
        static PlaylistService _instance;
        /// <summary>
        /// 获取当前播放列表实例
        /// </summary>
        public static PlaylistService Instance { get { return _instance ?? (_instance = new PlaylistService()); } }

        private ObservableCollection<SongViewModel> _Playlist;
        /// <summary>
        /// 获取播放列表的唯一实例
        /// </summary>
        public ObservableCollection<SongViewModel> Playlist
        {
            get
            {
                if (_Playlist == null)
                    InitPlaylist();
                return _Playlist;
            }
        }
        /// <summary>
        /// 应用程序开始时初始化播放列表
        /// </summary>
        /// <returns>上一次程序退出时的播放列表</returns>
        public async void InitPlaylistAsync()
        {
            //TODO: 从应用设置中获取缓存的播放列表
            //以下为测试代码
            //TODO: 如果从网络获取的话由于该方法不会被等待，因此需要考虑延后，如果从缓存中获取则取消async标志

            //AlbumModel am = AlbumModel.GetNew(1311688232);
            //int i = 0;
            //System.Diagnostics.Debugger.Break();
            //await JacobC.Xiami.Net.WebApi.Instance.GetAlbumInfo(am);
            //_Playlist = am.SongList.Select((sm) => new SongViewModel(sm) { ListIndex = i++ }).ToObservableCollection();
            _Playlist = new ObservableCollection<SongViewModel>();
        }

        private MediaPlaybackList _Medialist;
        /// <summary>
        /// 获取用于后台Mediaplayer的播放列表的唯一实例
        /// </summary>
        public MediaPlaybackList Medialist
        {
            get
            {
                if (_Playlist == null)
                    InitPlaylist();
                return _Medialist;
            }
        }

        public void InitPlaylist()
        {
            int i = 0;
            var list = InitPlaylistE().ToList();
            _Playlist = list.Select(sm=>new SongViewModel(sm) { ListIndex = i++}).ToObservableCollection();
            _Medialist = new MediaPlaybackList();
            _Medialist.AutoRepeatEnabled = true;//待从设置修改
            _Medialist.Items.
        }
        public IEnumerable<SongModel> InitPlaylistE()
        {
            //以下为测试代码
            for (int i = 0; i < 6; i++)
            {
                SongModel sm = SongModel.GetNew(1775616994);
                sm.Name = $"Foundations (Original Mix){i}";
                sm.Album = AlbumModel.GetNew(2100274906);
                sm.MediaUri = new Uri(@"ms-appx:///Assets/TestMedia/Ring01.wma");
                sm.Album.AlbumArtUri = new Uri("http://img.xiami.net/images/album/img35/105735/21002749061455506376_2.jpg");
                yield return sm;
                sm = SongModel.GetNew(1770914850);
                sm.Name = $"Give My Regards{i}";
                sm.Album = AlbumModel.GetNew(504506);
                sm.MediaUri = new Uri(@"ms-appx:///Assets/TestMedia/Ring02.wma");
                sm.Album.AlbumArtUri = new Uri("http://img.xiami.net/images/album/img35/105735/5045061333262175_2.jpg");
                yield return sm;
            }
        }
        /// <summary>
        /// 获取播放列表的Model列表
        /// </summary>
        public IEnumerable<SongModel> GetModelList() => Playlist.Select((source) => source.Model);

    }
}
