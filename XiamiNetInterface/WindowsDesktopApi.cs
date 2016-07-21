using HtmlAgilityPack;
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 虾米Windows桌面端使用的Api集（实际上桌面程序也大量使用了Web的api）
    /// </summary>
    public class WindowsDesktopApi : IXiamiInterface
    {
        /*TODO:
         * 未登录版本
         * av版本号可以没有，uid也可以没有 
         * /app/xiating/collect-recommend?av=XMusic_2.0.2.1618&uid=0 推荐精选集
         * /app/xiating/home-hot2?home_type=1&av=XMusic_2.0.2.1618&uid=0 热门专辑+热门艺人
         * /app/xiating/home-hot2?home_type=2&av=XMusic_2.0.2.1618&uid=0 推荐专辑+推荐艺人
         * /app/xiating/hot-music?av=XMusic_2.0.2.1618&uid=0 虾米音乐榜
         * 
         * 以下内容为api页面调用
         * /pc/下存放桌面版有关的样式和js文件
         * /api?api_key=..&api_sig=..&call_id=..&method=...&type=...&ver=...&page=.. api目录，返回json 其中如果method是Search.hotWords则是获取热词
         * 
         * 登录
         * /api/oauth2/token?grant_type=password&client_id=....&username=...&password=密码加密了  ，返回json，含有token
         * 
         * 登录以后
         * 
         * 登录以后/app/的页面uid=用户uid
         * 登录以后可以使用的api的method有Library.getLibrary, Library.getSongs（收藏的曲目）, Library.getCollects, Members.token， Search.autocomplete2, limit控制一页多少个，page可以控制页数
         */

        private WindowsDesktopApi() { }
        static WindowsDesktopApi _instance;
        /// <summary>
        /// 获取<see cref="WindowsDesktopApi"/>的唯一实例
        /// </summary>
        public static WindowsDesktopApi Instance
        {
            get
            {
                if (_instance == null) _instance = new WindowsDesktopApi();
                return _instance;
            }
        }

        //Completed
        /// <summary>
        /// 通过SongId获取歌曲的信息（不含取媒体地址）
        /// </summary>
        public IAsyncAction GetSongInfo(SongModel song)
        {
            if (song.SongID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/app/xiating/song?id={song.SongID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    HtmlNode root = doc.DocumentNode;
                    var logo = root.SelectSingleNode("//img[1]");
                    var detail = root.SelectSingleNode("//ul[1]");
                    var detailgrade = root.SelectSingleNode("//div[1]/ul[1]");
                    if (song.Title == null)
                        song.Title = logo.GetAttributeValue("title", "UnKnown");
                    song.PlayCount = int.Parse(detailgrade.SelectSingleNode(".//span[1]").InnerText);
                    song.ShareCount = int.Parse(detailgrade.SelectSingleNode("./li[3]/span[1]").InnerText);
                    if (song.Album == null)
                    {
                        AlbumModel album = new AlbumModel();
                        var art = logo.GetAttributeValue("src", "ms-appx:///Assets/Pictures/cd100.gif");
                        album.AlbumArtUri = new Uri(art.Replace("_2", "_1"));
                        album.AlbumArtFullUri = new Uri(art.Replace("_2", ""));
                        var albumtag = detail.SelectSingleNode("./li[1]/a[1]");
                        var idtext = albumtag.GetAttributeValue("href", "/app/xiating/album?id=0");
                        var addrlength = "/app/xiating/album?id=".Length;
                        album.AlbumID = uint.Parse(idtext.Substring(addrlength, idtext.IndexOf("&", addrlength) - addrlength));
                        album.Name = albumtag.InnerText;
                        song.Album = album;
                    }
                    if (song.Artist == null)
                    {
                        ArtistModel artist = new ArtistModel();
                        var artisttag = detail.SelectSingleNode("./li[2]/a[1]");
                        var idtext = artisttag.GetAttributeValue("href", "/app/xiating/artist?id=0");
                        var addrlength = "/app/xiating/artist?id=".Length;
                        artist.ArtistID = uint.Parse(idtext.Substring(addrlength, idtext.IndexOf("&", addrlength) - addrlength));
                        artist.Name = artisttag.InnerText;
                        song.Artist = artist;
                    }
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e);
                    throw e;
                }
            });
        }

        public IAsyncAction GetAlbumInfo(AlbumModel album)
        {
            if (album.AlbumID == 0)
                throw new ArgumentException("AlbumModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/app/xiating/album?id={album.AlbumID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e);
                    throw e;
                }
            });
        }

        public IAsyncAction GetArtistInfo(ArtistModel artist)
        {
            if (artist.ArtistID == 0)
                throw new ArgumentException("ArtistModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/app/xiating/artist?id={artist.ArtistID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e);
                    throw e;
                }
            });
        }
    }
}
