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
    public class PlaylistService
    {
        //TODO: 尝试其他方法完成无缝播放，系统的mediaplayer是不会缓存下一轨的(如果缓存可以将所有播放任务均交给后台)
        readonly Uri IdleSongPath = new Uri("http://www.tonycuffe.com/mp3/tail%20toddle.mp3");

        static PlaylistService _instance;
        /// <summary>
        /// 获取当前播放列表实例
        /// </summary>
        public static PlaylistService Instance { get { return _instance ?? (_instance = new PlaylistService()); } }

        SongModel _CurrentPlaying = null;
        /// <summary>
        /// 获取当前选中或播放的音轨
        /// </summary>
        public SongModel CurrentPlaying
        {
            get
            {
                if (Playlist.Count == 0)
                    throw new ArgumentNullException(nameof(CurrentPlaying), "当前播放列表为空，无法获取音轨");
                return _CurrentPlaying;
            }
            set
            {
                if (_CurrentPlaying != value)
                {
                    CurrentIndexChanging.Invoke(this, new ChangedEventArgs<SongModel>(_CurrentPlaying, value));
                    InternalCurrentIndexChanging(value);
                    _CurrentPlaying = value;
                }
            }
        }
        /// <summary>
        /// 在当前播放的音轨发生改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<SongModel>> CurrentIndexChanging;
        private void InternalCurrentIndexChanging(SongModel newsong)
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
                sm.Name = $"Foundations (Original Mix){i}";
                sm.Album = AlbumModel.GetNew(2100274906);
                sm.MediaUri = new Uri(@"ms-appx:///Assets/TestMedia/Ring01.wma");
                sm.Album.AlbumArtUri = new Uri("http://img.xiami.net/images/album/img35/105735/21002749061455506376_2.jpg");
                yield return sm;
                sm = SongModel.GetNew(1770914850);
                sm.Name = $"Give My Regards{i}";
                sm.Album = AlbumModel.GetNew(504506);
                sm.MediaUri = IdleSongPath;
                sm.Album.AlbumArtUri = new Uri("http://img.xiami.net/images/album/img35/105735/5045061333262175_2.jpg");
                yield return sm;
            }
        }

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
