using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Utils;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 维护全局的播放列表集合
    /// </summary>
    public class PlaylistService
    {
        private ObservableCollection<SongModel> _Playlist;
        /// <summary>
        /// 获取播放列表的唯一实例
        /// </summary>
        public ObservableCollection<SongModel> Playlist
        {
            get
            {
                //if (_Playlist != null)
                //    return _Playlist;
                return _Playlist = _Playlist ?? InitPlaylist().ToObservableCollection();
            }
        }
        /// <summary>
        /// 应用程序开始时初始化播放列表
        /// </summary>
        /// <returns>上一次程序退出时的播放列表</returns>
        public IEnumerable<SongModel> InitPlaylist()
        {
            //TODO: 从应用设置中获取缓存的播放列表
            throw new NotImplementedException();
        }
    }
}
