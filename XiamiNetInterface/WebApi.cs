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
                    var loveop = body.SelectSingleNode(".//ul/li[1]");
                    song.IsLoved = loveop.GetAttributeValue("style", "") == "display:none";
                    var detail = body.SelectSingleNode(".//table");
                    foreach (var item in detail.SelectNodes(".//tr"))
                    {
                        switch(item.ChildNodes[1].InnerText)
                        {
                            case "所属专辑：":
                                var linknode = item.SelectSingleNode(".//a");
                                var id = uint.Parse(linknode.GetAttributeValue("href", "/album/0").Substring(7));
                                if ((song.Album == null )|| cover)
                                {
                                    var album = song.Album ?? AlbumModel.GetNew(id);
                                    album.Name = linknode.InnerText;
                                    if (album.AlbumArtUri.Host == "")
                                    {
                                        var art = detail.ParentNode.SelectSingleNode(".//img").GetAttributeValue("src", "ms-appx:///Assets/Pictures/cd100.gif");
                                        album.AlbumArtUri = new Uri(art.Replace("_2", "_1"));
                                        album.AlbumArtFullUri = new Uri(art.Replace("_2", ""));
                                    }
                                    song.Album = album;
                                }
                                break;
                            case "演唱者：":
                                song.TrackArtist = item.SelectSingleNode(".//a").InnerText;
                                break;
                            case "作词：":
                                song.Lyricist = item.SelectSingleNode(".//div").InnerText;
                                break;
                            case "作曲：":
                                song.Composer = item.SelectSingleNode(".//div").InnerText;
                                break;
                            case "编曲：":
                                song.Arranger = item.SelectSingleNode(".//div").InnerText;
                                break;
                        }
                    }
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