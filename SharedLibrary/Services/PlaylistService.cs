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
        private static ObservableCollection<SongModel> _Playlist;
        /// <summary>
        /// 获取播放列表的唯一实例
        /// </summary>
        public static ObservableCollection<SongModel> Playlist
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
        public static IEnumerable<SongModel> InitPlaylist()
        {
            //TODO: 从应用设置中获取缓存的播放列表
            //以下为测试代码
            ArtistModel mitis = new ArtistModel() { Name = "MitiS" };
            for (int i = 0; i < 6; i++)
            {
                yield return new SongModel() { Title = "Foundations", Artist = mitis, Album = new AlbumModel() { Name = "Foundations" }, MediaUri = new Uri(@"http://win.web.rh03.sycdn.kuwo.cn/9cde1835bc61fe36d11291d29b43ae2e/5788f9ab/resource/a2/21/1/314466624.aac") };
                yield return new SongModel() { Title = "Give My Regards", Artist = mitis, Album = new AlbumModel() { Name = "Give My Regards" }, MediaUri = new Uri(@"http://win.web.rb03.sycdn.kuwo.cn/cf204eb40a150109cc4eb0f2c416c2ec/5789005e/resource/a3/73/65/3736166827.aac") };
            }
        }
    }
}
