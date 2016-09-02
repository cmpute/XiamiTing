using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JacobC.Xiami.Models;
using Template10.Common;

namespace JacobC.Xiami.Services
{
    /// <summary>
    /// 提供播放电台的服务
    /// </summary>
    public class RadioService : Queue<SongModel> ,IPlaylist
    {
        /// <summary>
        /// 从RadioModel构造Radio的播放服务
        /// </summary>
        public RadioService(RadioModel radio)
        {
            Radio = radio;
        }
        /// <summary>
        /// 该RadioService对应的<see cref="RadioModel"/>模型
        /// </summary>
        public RadioModel Radio { get; private set; }

        SongModel current = null;
        /// <summary>
        /// 获取当前正在播放的音轨
        /// </summary>
        public SongModel CurrentPlaying
        {
            get
            {
                if (current == null)
                    ExtensionMethods.InvokeAndWait(async () => await PlayNext());
                return current;
            }
            private set
            {
                if (current != value)
                {
                    var e = new ChangedEventArgs<SongModel>(current, value);
                    current = value;
                    CurrentPlayingChanged?.Invoke(this, e);
                }
            }
        }
        /// <summary>
        /// 当正在播放的音轨发生变化时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<SongModel>> CurrentPlayingChanged;

        public async Task PlayNext()
        {
            if (Count == 0)
                await Net.WebApi.Instance.FreshRadio(this);
            CurrentPlaying = Dequeue();
        }
    }
    //TODO: 多个Radio混合播放
}