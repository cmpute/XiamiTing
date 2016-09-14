using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class ListenLogModel
    {
        public ListenLogModel(SongModel song, LogDevices device, string logdate)
        {
            Song = song;
            Device = device;
            LogDateDiff = logdate;
        }
        /// <summary>
        /// 获取或设置歌曲
        /// </summary>
        public SongModel Song { get; set; }
        /// <summary>
        /// 获取或设置听歌设备
        /// </summary>
        public LogDevices Device { get; set; }
        /// <summary>
        /// 获取或设置记录时间相对于现在的时间
        /// </summary>
        public string LogDateDiff { get; set; }
    }

    //public class ListenLogModel : SafeBindableBase
    //{
    //    public ListenLogModel()

    //    SongModel _Song = default(SongModel);
    //    /// <summary>
    //    /// 获取或设置歌曲
    //    /// </summary>
    //    public SongModel Song { get { return _Song; } set { Set(ref _Song, value); } }

    //    ArtistModel _Artist = default(ArtistModel);
    //    /// <summary>
    //    /// 获取或设置艺人对象
    //    /// </summary>
    //    public ArtistModel Artist { get { return _Artist; } set { Set(ref _Artist, value); } }

    //    LogDevices _Device = default(LogDevices);
    //    /// <summary>
    //    /// 获取或设置听歌设备
    //    /// </summary>
    //    public LogDevices Device { get { return _Device; } set { Set(ref _Device, value); } }

    //    string _LogDateDiff = default(string);
    //    /// <summary>
    //    /// 获取或设置记录时间相对于现在的时间
    //    /// </summary>
    //    public string LogDateDiff { get { return _LogDateDiff; } set { Set(ref _LogDateDiff, value); } }
    //}

    /// <summary>
    /// 听歌记录的来源
    /// </summary>
    public enum LogDevices
    {
        Web,
        Android,
        iOS,
    }
}
