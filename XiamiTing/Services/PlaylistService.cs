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
                {
                    _Playlist = InitPlaylist().ToObservableCollection();
                }
                return _Playlist;
            }
        }
        /// <summary>
        /// 应用程序开始时初始化播放列表
        /// </summary>
        /// <returns>上一次程序退出时的播放列表</returns>
        public IEnumerable<SongViewModel> InitPlaylist()
        {
            //TODO: 从应用设置中获取缓存的播放列表
            //以下为测试代码
            ArtistModel mitis = new ArtistModel() { Name = "MitiS" };
            for (int i = 0; i < 6; i++)
            {
                SongViewModel vm = new SongViewModel(new SongModel() { Title = $"Give My Regards {i}", Artist = mitis, Album = new AlbumModel() { Name = "Give My Regards", AlbumArtUri = new Uri(@"ms-appx:///Assets/TestMedia/Ring01.jpg") }, MediaUri = new Uri(@"http://win.web.rb03.sycdn.kuwo.cn/3c7436b07688ca96d1cfb9bc6a547706/578f5562/resource/a3/73/65/3736166827.aac") });
                vm.ListIndex = i;
                yield return vm;
                vm = new SongViewModel(new SongModel() { Title = $"Foundations {i}", Artist = mitis, Album = new AlbumModel() { Name = "Foundations", AlbumArtUri=new Uri("http://blog.51cto.com/images/special/1306310178_index.jpg") }, MediaUri = new Uri(@"ms-appx:///Assets/TestMedia/Ring02.wma") });
                vm.ListIndex = i;
                yield return vm;
            }
        }
        /// <summary>
        /// 获取播放列表的Model列表
        /// </summary>
        public Collection<SongModel> GetModelList()
        {
            Collection<SongModel> list = new Collection<SongModel>();
            foreach (SongViewModel s in Playlist)
                list.Add(s.Model);
            return list;
        }
    }
}
