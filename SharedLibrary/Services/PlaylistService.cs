using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Utils;

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

        private ObservableCollection<SongModel> _Playlist;
        /// <summary>
        /// 获取播放列表的唯一实例
        /// </summary>
        public ObservableCollection<SongModel> Playlist
        {
            get
            {
                if(_Playlist == null)
                {
                    _Playlist = InitPlaylist().ToObservableCollection();
                    //初始化以后更新UI
                    CurrentIndexChanging.Invoke(this, new ChangedEventArgs<int>(0, 0));
                }
                return _Playlist;
            }
        }
        /// <summary>
        /// 应用程序开始时初始化播放列表
        /// </summary>
        /// <returns>上一次程序退出时的播放列表</returns>
        public IEnumerable<SongModel> InitPlaylist()
        {
            //TODO: 从应用设置中获取缓存的播放列表
            //以下为测试代码
            ArtistModel mitis = new ArtistModel() { Name = "MitiS" };
            for (int i = 0; i < 6; i++)
            {
                yield return new SongModel() { Title = "Give My Regards", Artist = mitis, Album = new AlbumModel() { Name = "Give My Regards" }, MediaUri = new Uri(@"http://win.web.rb03.sycdn.kuwo.cn/4450c3aa50371db1f4cd52953039cd58/5789ab7d/resource/a3/73/65/3736166827.aac") };
                yield return new SongModel() { Title = "Foundations", Artist = mitis, Album = new AlbumModel() { Name = "Foundations" }, MediaUri = new Uri(@"http://win.web.rh03.sycdn.kuwo.cn/9cde1835bc61fe36d11291d29b43ae2e/5788f9ab/resource/a2/21/1/314466624.aac") };
            }
        }

        int _CurrentIndex = 0;
        /// <summary>
        /// 获取当前选中或播放的音轨
        /// </summary>
        public int CurrentIndex
        {
            get
            {
                if (_Playlist.Count == 0)
                    throw new ArgumentNullException(nameof(CurrentIndex), "当前播放列表为空，无法获取音轨");
                return _CurrentIndex;
            }
            set
            {
                if (_CurrentIndex != value)
                {
                    CurrentIndexChanging.Invoke(this, new ChangedEventArgs<int>(_CurrentIndex, value));
                    InternalCurrentIndexChanging(value);
                    _CurrentIndex = value;
                }
            }
        }
        /// <summary>
        /// 在当前播放的音轨发生改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<int>> CurrentIndexChanging;
        private void InternalCurrentIndexChanging(int newindex)
        {
            //TODO: 向后台发送消息
        }
    }
}
