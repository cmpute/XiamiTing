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
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 维护全局的播放列表集合
    /// </summary>
    public class PlaylistService// : ObservableCollection<SongModel>
    {
        //TODO: 尝试其他方法完成无缝播放，系统的mediaplayer是不会缓存下一轨的(如果缓存可以将所有播放任务均交给后台)
        readonly Uri IdleSongPath = new Uri("http://win.web.rh03.sycdn.kuwo.cn/5732a6217291881c369bc1b89b620f1b/57b99dd7/resource/a1/7/41/1607541714.aac");

        #region Ctor
        static PlaylistService _instance;
        /// <summary>
        /// 获取当前播放列表实例
        /// </summary>
        public static PlaylistService Instance { get { return _instance ?? (_instance = new PlaylistService()); } }

        private PlaylistService()
        {
            CurrentIndex = SettingsService.Instance.Playback.ReadAndReset("TrackID", -1);
        }
        #endregion

        /// <summary>
        /// 获取当前选中或播放的音轨
        /// </summary>
        public SongModel CurrentPlaying
        {
            get
            {
                if (CurrentIndex == -1)
                    return null;
                    //throw new ArgumentNullException(nameof(CurrentPlaying), "当前播放列表为空，无法获取音轨");
                return Playlist[CurrentIndex];
            }
        }

        int _currentIndex = -1;
        /// <summary>
        /// 获取或设置当前播放的音轨号
        /// </summary>
        /// <remarks>
        /// 暂停的时候音轨号保留，但是单次播放以后音轨号不保留
        /// 音轨号与音轨的显示状态有关
        /// </remarks>
        public int CurrentIndex
        {
            get
            {
                if (Playlist.Count == 0)
                    return -1;
                return _currentIndex;
            }
            set
            {
                if(_currentIndex != value)
                {
                    var e = new ChangedEventArgs<int>(_currentIndex, value);
                    _currentIndex = value;
                    CurrentIndexChanged?.Invoke(this, e);
                    InternalCurrentIndexChanged(value);
                }
            }
        }
        /// <summary>
        /// 在当前播放的音轨发生改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<int>> CurrentIndexChanged;
        private void InternalCurrentIndexChanged(int newindex)
        {
            //TODO: 向后台发送消息
        }

        private ObservableCollection<SongModel> _Playlist;
        /// <summary>
        /// 获取播放列表的唯一实例
        /// </summary>
        public ObservableCollection<SongModel> Playlist
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
            //_Playlist = am.SongList.Select((sm) => new SongModel(sm) { ListIndex = i++ }).ToObservableCollection();
            //_Playlist = new ObservableCollection<SongModel>();
            await Task.CompletedTask;
        }

        //获取下一首播放的歌曲，提前计算地址
        private SongModel GetNext()
        {
            throw new NotImplementedException();
        }

        private void InitPlaylist()
        {
            _Playlist = InitPlaylistE().ToObservableCollection();
        }

        public IEnumerable<SongModel> InitPlaylistE()
        {
            //以下为测试代码
            for (int i = 0; i < 6; i++)
            {
                SongModel sm = SongModel.GetNew(1775616994);
                sm.Name = $"Foundations (Original Mix)";
                sm.Album = AlbumModel.GetNew(2100274906);
                sm.MediaUri = new Uri(@"ms-appx:///Assets/Ring01.wma");
                sm.Album.AlbumArtUri = new Uri("http://img.xiami.net/images/album/img35/105735/21002749061455506376_2.jpg");
                yield return sm;
                sm = SongModel.GetNew(1770914850);
                sm.Name = $"Give My Regards";
                sm.Album = AlbumModel.GetNew(504506);
                sm.MediaUri = new Uri(@"ms-appx:///Assets/Ring02.wma");
                sm.Album.AlbumArtUri = new Uri("http://img.xiami.net/images/album/img35/105735/5045061333262175_2.jpg");
                yield return sm;
            }
        }

        #region Playback Order

        //TODO: 打乱时保证正在播放的项目为第一首
        //TODO: 打乱后更改CurrentIndex
        /// <summary>
        /// 打乱当前的播放列表
        /// </summary>
        public void ShuffleListInPlace()
        {
            //var t = ShuffleList();
            //List<SongModel> temp = new List<SongModel>();
            //foreach (var item in t)
            //    temp.Add(_Playlist[item]);
            //_Playlist.AddRange(temp, true);
            
            int total = _Playlist.Count, i;
            Random random = new Random();
            while (total > 0)
            {
                i = random.Next(total--);
                var t = _Playlist[total];
                _Playlist[total] = _Playlist[i];
                _Playlist[i] = t;
            }
        }
        /// <summary>
        /// 获取一个随机播放的序号表
        /// </summary>
        /// <returns>用于随机播放的不重复编号列表</returns>
        public IList<int> ShuffleList()
        {
            Random random = new Random();
            int total = _Playlist.Count;
            int[] sequence = new int[total];
            int[] output = new int[total];
            for (int i = 0; i < total; i++)
                sequence[i] = i;
            int end = total - 1;
            for (int i = 0; i < total; i++)
            {
                int num = random.Next(0, end + 1);
                output[i] = sequence[num];
                sequence[num] = sequence[end];
                end--;
            }
            return output;
        }
        /// <summary>
        /// 获取或设置列表的播放顺序
        /// </summary>
        public PlayOrder PlaybackOrder
        {
            get; set;
        }

        #endregion

    }

    /// <summary>
    /// 标识播放顺序
    /// </summary>
    public enum PlayOrder
    {
        /// <summary>
        /// 顺序播放
        /// </summary>
        Default,
        /// <summary>
        /// 重复单轨
        /// </summary>
        Repeat,
        /// <summary>
        /// 循环播放列表
        /// </summary>
        RepeatList
    }
}
