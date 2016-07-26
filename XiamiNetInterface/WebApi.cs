using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using Windows.Web.Http;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using HtmlAgilityPack;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 虾米Web端使用的Api集
    /// </summary>
    public class WebApi : IXiamiApi
    {
        /* TODO:
         * 
         */
        private WebApi() { }
        static WebApi _instance;
        /// <summary>
        /// 获取<see cref="WebApi"/>的唯一实例
        /// </summary>
        public static WebApi Instance
        {
            get
            {
                if (_instance == null) _instance = new WebApi();
                return _instance;
            }
        }

        public IAsyncAction GetSongInfo(SongModel song, bool cover = false)
        {
            if (song.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Song {song.XiamiID}", "NetInterface");

                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/song/{song.XiamiID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode("//div[@id='page']");
                    var title = body.SelectSingleNode(".//h1");
                    if (song.Name == null || cover)
                        song.Name = title.FirstChild.InnerText;
                    if (song.Description == null || cover)
                        song.Description = title.LastChild.InnerText;

                    //System.Diagnostics.Debug.Write(body.OuterHtml);
                    //System.Diagnostics.Debugger.Break();
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, "NetInterface");
                    throw e;
                }
            });
        }

        public IAsyncAction GetAlbumInfo(AlbumModel album, bool cover = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction GetArtistInfo(ArtistModel artist, bool cover = false)
        {
            throw new NotImplementedException();
        }
    }
}