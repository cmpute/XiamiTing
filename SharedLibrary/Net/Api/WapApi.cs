using HtmlAgilityPack;
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 虾米旧版移动客户端使用的Api集
    /// </summary>
    /// <remarks>
    /// Api弃置
    /// </remarks>
    public class WapApi : IXiamiApi
    {
        /* TODO:
         * 未登录版本
         * uid也可以没有 cookie中设置av=4.5信息量超级多！！！
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

        private WapApi() { }
        static WapApi _instance;
        /// <summary>
        /// 获取<see cref="WapApi"/>的唯一实例
        /// </summary>
        public static WapApi Instance
        {
            get
            {
                if (_instance == null) _instance = new WapApi();
                return _instance;
            }
        }

        //TODO: 判断歌曲是否被喜爱
        /// <summary>
        /// 通过SongId获取歌曲的信息（不含取媒体地址）
        /// </summary>
        /// <param name="cover">是否覆盖已存在的Album和Artist信息</param> 
        public IAsyncAction GetSongInfo(SongModel song, bool cover = false)
        {
            if (song.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Song {song.XiamiID}", nameof(WapApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/app/xiating/song?id={song.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    HtmlNode root = doc.DocumentNode;
                    var logo = root.SelectSingleNode("//img[1]");
                    var detail = root.SelectSingleNode("//ul[1]");
                    var detailgrade = root.SelectSingleNode("//div[1]/ul[1]");
                    if (song.Name == null)
                        song.Name = logo.GetAttributeValue("title", "UnKnown");
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
                        if (album.Art.Host == "")
                        {
                            var art = logo.GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                            album.Art = new Uri(art.Replace("_2", "_1"));
                            album.ArtFull = new Uri(art.Replace("_2", ""));
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

                    LogService.DebugWrite($"Finish Getting info of Song {song.Name}", nameof(WapApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WapApi));
                    throw e;
                }
            });
        }

        /// <summary>
        /// 通过AlbumId获取专辑信息（不含歌曲列表）
        /// </summary>
        /// <param name="cover">是否覆盖已存在的Artist信息</param>
        public IAsyncAction GetAlbumInfo(AlbumModel album, bool cover = false)
        {
            if (album.XiamiID == 0)
                throw new ArgumentException("AlbumModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Album {album.XiamiID}", nameof(WapApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/app/xiating/album?id={album.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    List<Task> process = new List<Task>();//并行处理
                    process.Add(Task.Run(() => { if (album.SongList == null || cover) album.SongList = ParseAlbumSongs(doc.DocumentNode.SelectSingleNode("//div/ul[1]"), album).ToList(); }));
                    process.Add(Task.Run(() => { if (album.RelateHotAlbums == null || cover) album.RelateHotAlbums = ParseRelateAlbums(doc.DocumentNode.SelectSingleNode("//h3").NextSibling.NextSibling).ToList(); }));

                    var infonode = doc.DocumentNode.SelectSingleNode("//section[1]/div[1]/div[2]/div[1]");
                    if (album.Art.Host == "")
                    {
                        var art = infonode.SelectSingleNode(".//img").GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                        album.Art = new Uri(art);
                        album.ArtFull = new Uri(art.Replace("_1", ""));
                    }
                    album.Name = infonode.SelectSingleNode(".//h2").InnerText;
                    album.Rating = infonode.SelectSingleNode(".//p").InnerText.Remove(0, 4).Trim();
                    album.ReleaseDate = infonode.SelectSingleNode(".//span/span").InnerText.Remove(0, 5);//TODO: 针对地域进行转换
                    var artisttag = infonode.SelectSingleNode(".//span/a");
                    if ((album.Artist==null)||cover)
                    {
                        var idtext = artisttag.GetAttributeValue("onclick", "artist_detail(0);");
                        var addrlength = "artist_detail(".Length;
                        uint artistID = uint.Parse(idtext.Substring(addrlength, idtext.IndexOf(")", addrlength) - addrlength));
                        ArtistModel artist = album.Artist ?? ArtistModel.GetNew(artistID);
                        artist.Name = artisttag.InnerText;
                        album.Artist = artist;
                    }
                    
                    await Task.WhenAll(process);
                    LogService.DebugWrite($"Finish Getting info of Album {album.Name}", nameof(WapApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e);
                    throw e;
                }
            });
        }
        /* 换用别的传递内容或者返回新的List减少内存占用？listnode的引用会导致GC无法清理HtmlDocument?
         * 实测发现返回换成返回一个新List也没有减少内存占用。。
         */
        internal IEnumerable<SongModel> ParseAlbumSongs(HtmlNode listnode, AlbumModel album)
        {
            foreach(var node in listnode.ChildNodes)
            {
                if (node.NodeType != HtmlNodeType.Element)
                    continue;
                uint songID = uint.Parse(node.SelectSingleNode("./span").GetAttributeValue("rel", "0"));
                SongModel song = SongModel.GetNew(songID);
                song.Name = node.SelectSingleNode("./div/a").InnerText;
                song.TrackArtist = node.SelectSingleNode("./div/span")?.InnerText;
                song.PlayCount = int.Parse(node.SelectSingleNode("./div[2]/span").InnerText);
                song.Album = album;
                yield return song;
            }
        }
        internal IEnumerable<AlbumModel> ParseRelateAlbums(HtmlNode listnode)
        {
            foreach (var node in listnode.ChildNodes)
            {
                if (node.NodeType != HtmlNodeType.Element)
                    continue;
                AlbumModel album = AlbumModel.GetNew(uint.Parse(node.GetAttributeValue("rel", "0")));
                album.Name = node.SelectSingleNode("./div/a").InnerText;
                album.Rating = node.SelectSingleNode(".//em").InnerText;
                if (album.Art.Host == "")
                {
                    var art = node.SelectSingleNode(".//img").GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                    album.Art = new Uri(art);
                    album.ArtFull = new Uri(art.Replace("_1", ""));
                }
                yield return album;
            }
        }

        public IAsyncAction GetArtistInfo(ArtistModel artist, bool cover = false)
        {
            //TODO: 艺人专辑http://www.xiami.com/app/xiating/artist-album2?id= 相似艺人http://www.xiami.com/app/xiating/artist-similar?id=
            //TODO: 艺人专辑、艺人歌曲列表都采用增量加载
            if (artist.XiamiID == 0)
                throw new ArgumentException("ArtistModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Artist {artist.XiamiID}", nameof(WapApi));
                    
                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/app/xiating/artist?id={artist.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var body = doc.DocumentNode.SelectSingleNode("//div[@id='artist']");
                    artist.Name = body.SelectSingleNode(".//h2").InnerText;
                    artist.AliasName = body.SelectSingleNode(".//p").InnerText;
                    var area = body.SelectSingleNode(".//p[2]");
                    if (area != null) artist.Name = area.InnerText.Remove(0, 3);

                    if (artist.Art.Host == "")
                    {
                        var art = body.SelectSingleNode(".//img").GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                        artist.Art = new Uri(art);
                        artist.ArtFull = new Uri(art.Replace("_1", ""));
                    }
                    
                    var songlist = ParseArtistSongs(body.SelectSingleNode(".//ul[@class='playlist']")).ToArray();//只计算一次count
                    artist.HotSongs = new PageItemsCollection<SongModel>(songlist, (pageindex, c) => GetArtistSongsPage(artist.XiamiID, pageindex, c));
                    artist.Albums = new PageItemsCollection<AlbumModel>(16, (pageindex, c) => GetArtistAlbumPage(artist.XiamiID, pageindex, c));
                    
                    LogService.DebugWrite($"Finish Getting info of Artist {artist.XiamiID}", nameof(WapApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WapApi));
                    throw e;
                }
            });
        }
        internal IEnumerable<SongModel> ParseArtistSongs(HtmlNode listnode)
        {
            foreach(var item in listnode.ChildNodes)
            {
                if (item.NodeType != HtmlNodeType.Element)
                    continue;
                var id = uint.Parse(item.SelectSingleNode("./span").GetAttributeValue("rel", "0"));
                SongModel song = SongModel.GetNew(id);
                song.Name = item.SelectSingleNode(".//a").InnerText;
                int playcount = -1;
                if (int.TryParse(item.SelectSingleNode("./div/span").InnerText, out playcount))
                    song.PlayCount = playcount;
                yield return song;
            }
        }
        internal Task<IEnumerable<SongModel>> GetArtistSongsPage(uint artistId, uint pageindex, CancellationToken c)
        {
            LogService.DebugWrite($"Get Artist Song Page{pageindex}", nameof(WapApi));
            return Task.Run(async () =>
            {
                try
                {
                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/app/xiating/artist?id={artistId}&page={pageindex}&callback=JQuery");
                    c.Register(() => gettask.Cancel());
                    var content = System.Text.RegularExpressions.Regex.Unescape(await gettask);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content.Substring(8, content.Length - 10));
                    return ParseArtistSongs(doc.DocumentNode);
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WapApi));
                    throw e;
                }
            });
        }
        internal Task<IEnumerable<AlbumModel>> GetArtistAlbumPage(uint artistId, uint pageindex, CancellationToken c)
        {
            LogService.DebugWrite($"Get Artist Album Page{pageindex}", nameof(WapApi));
            return Task.Run(async () =>
            {
                try
                {
                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/app/xiating/artist-album2?id={artistId}&page={pageindex}&callback=JQuery");
                    c.Register(() => gettask.Cancel());
                    var content = System.Text.RegularExpressions.Regex.Unescape(await gettask);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content.Substring(8, content.Length - 10));
                    return ParseArtistAlbums(doc.DocumentNode);
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WapApi));
                    throw e;
                }
            });
        }
        internal IEnumerable<AlbumModel> ParseArtistAlbums (HtmlNode listnode)
        {
            foreach (var item in listnode.ChildNodes)
            {
                if (item.NodeType != HtmlNodeType.Element)
                    continue;
                var id = uint.Parse(item.GetAttributeValue("rel", "0"));
                AlbumModel album = AlbumModel.GetNew(id);
                var imagenode = item.SelectSingleNode(".//img");
                album.Name = imagenode.GetAttributeValue("alt", null);
                if(album.Art.Host == "")
                {
                    var art = imagenode.GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                    album.Art = new Uri(art);
                    album.ArtFull = new Uri(art.Replace("_1", ""));
                }
                album.Rating = item.SelectSingleNode(".//em").InnerText;
                yield return album;
            }
        }
    }
}
