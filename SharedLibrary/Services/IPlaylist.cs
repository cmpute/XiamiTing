using JacobC.Xiami.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Services
{
    public interface IPlaylist : ICollection, IEnumerable<SongModel>
    {
        /// <summary>
        /// 获取当前正在播放的歌曲
        /// </summary>
        SongModel CurrentPlaying { get; }
        //int CurrentIndex { get; }
        /// <summary>
        /// 当正在播放的歌曲发生改变时发生
        /// </summary>
        event EventHandler<Template10.Common.ChangedEventArgs<SongModel>> CurrentPlayingChanged;
    }
}
