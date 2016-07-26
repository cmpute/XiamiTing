using HtmlAgilityPack;
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// Windows桌面客户端使用的Api
    /// </summary>
    public class WindowsDesktopApi : IXiamiApi
    {
        //TODO: 需要增加cookieContainer，av=4.5
        //TODO: 需考虑如何将歌曲分成Disc1，2，3
        //TODO: 需考虑PageLoadCollection是存SongModel还是SongViewModel

        private WindowsDesktopApi() { }
        static WindowsDesktopApi _instance;
        /// <summary>
        /// 获取<see cref="WindowsDesktopApi"/>的唯一实例
        /// </summary>
        public static WindowsDesktopApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowsDesktopApi();
                    //不能在同一次应用中使用两种Api
                    //TODO： 在更新Api选择后重置Handler的CookieContainer
                    HttpHelper.Handler.CookieContainer.Add(new Uri("http://www.xiami.com"), new System.Net.Cookie("av", "4.5"));
                }
                return _instance;
            }
        }

        public IAsyncAction GetSongInfo(SongModel song, bool cover = false)
        {
            if (song.SongID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {

                try
                {
                    LogService.DebugWrite($"Get info of Song {song.SongID}", "NetInterface");

                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/app/xiating/song?id={song.SongID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    HtmlNode root = doc.DocumentNode;
                    System.Diagnostics.Debug.WriteLine(root.OuterHtml);
                    System.Diagnostics.Debugger.Break();
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, "NetInterface");
                    throw e;
                }
            });
        }
    }
}
