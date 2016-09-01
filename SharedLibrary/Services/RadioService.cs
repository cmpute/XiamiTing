using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JacobC.Xiami.Models;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 提供播放电台的服务
    /// </summary>
    public class RadioService : Queue<SongModel> ,IPlaylist
    {
        public RadioService(RadioModel radio)
        {
            Radio = radio;
        }
        public RadioModel Radio { get; private set; }

        public SongModel CurrentPlaying { get; private set; }
        public void PlayNext()
        {
            CurrentPlaying = Dequeue();
        }

    }
    //TODO: 多个Radio混合播放
}