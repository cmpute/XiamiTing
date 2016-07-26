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
                    HtmlNode root = doc.DocumentNode.SelectSingleNode("//body");
                    var logo = root.SelectSingleNode(".//img[1]");
                    var detail = root.SelectSingleNode(".//ul[1]");
                    var detailgrade = root.SelectSingleNode(".//div[1]/ul[1]");
                    if (song.Title == null)
                        song.Title = logo.GetAttributeValue("title", "UnKnown");
                    if (song.AliasTitle == null)
                        song.AliasTitle = root.SelectSingleNode(".//p").InnerText;
                    song.PlayCount = int.Parse(detailgrade.SelectSingleNode(".//span[1]").InnerText);
                    song.ShareCount = int.Parse(detailgrade.SelectSingleNode("./li[3]/span[1]").InnerText);

                    var additionnodes = detail.SelectNodes("./li[position()>2]");
                    foreach (var node in additionnodes)
                        switch (node.FirstChild.InnerText)
                        {
                            case "作词：":
                                song.Lyricist = node.LastChild.InnerText;
                                break;
                            case "作曲：":
                                song.Composer = node.LastChild.InnerText;
                                break;
                            case "编曲：":
                                song.Arranger = node.LastChild.InnerText;
                                break;
                        }

                    if ((song.Album == null) || cover)
                    {
                        var albumtag = detail.SelectSingleNode("./li[1]/a[1]");
                        var idtext = albumtag.GetAttributeValue("href", "/app/xiating/album?id=0");
                        var addrlength = "/app/xiating/album?id=".Length;
                        uint albumID = uint.Parse(idtext.Substring(addrlength, idtext.IndexOf("&", addrlength) - addrlength));
                        AlbumModel album = song.Album ?? AlbumModel.GetNew(albumID);
                        if (album.AlbumArtUri.Host == "")
                        {
                            var art = logo.GetAttributeValue("src", "ms-appx:///Assets/Pictures/cd100.gif");
                            album.AlbumArtUri = new Uri(art.Replace("_2", "_1"));
                            album.AlbumArtFullUri = new Uri(art.Replace("_2", ""));
                        }
                        album.Name = albumtag.InnerText;
                        song.Album = album;
                    }

                    if ((song.Album?.Artist == null) || cover)
                    {
                        var artisttag = detail.SelectSingleNode("./li[2]/a[1]");
                        var idtext = artisttag.GetAttributeValue("href", "/app/xiating/artist?id=0");
                        var addrlength = "/app/xiating/artist?id=".Length;
                        uint artistID = uint.Parse(idtext.Substring(addrlength, idtext.IndexOf("&", addrlength) - addrlength));
                        ArtistModel artist = song.Album?.Artist ?? ArtistModel.GetNew(artistID);
                        artist.Name = artisttag.InnerText;
                        song.Album.Artist = artist;
                    }

                    LogService.DebugWrite($"Finish Getting info of Song {song.Title}", "NetInterface");
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
