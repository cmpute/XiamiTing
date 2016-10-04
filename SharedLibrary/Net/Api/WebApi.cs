using HtmlAgilityPack;
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 虾米Web端使用的Api集
    /// </summary>
    public class WebApi : IXiamiApi
    {
        #region Ctor & Common Methods
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

        internal bool CheckNeedLogin(string html) => html.Contains("id=\"login\"");
        internal void ParsePlayListSong(SongModel song, HtmlNode tr)
        {
            var name = tr.SelectSingleNode("./td[@class='song_name']");
            foreach (var item in name.ChildNodes)
            {
                if(item.Name == "a")
                {
                    if (item.Element("b") != null)
                    {
                        var mvlink = item.GetAttributeValue("href", "/0");
                        song.MV = MVModel.GetNew(mvlink.Substring(mvlink.LastIndexOf('/') + 1));
                    }
                    else if (item.GetAttributeValue("class", "") == "show_zhcn")
                        song.Description = item.InnerText;
                    else
                        song.Name = item.InnerText;
                }
                else 
                {
                    string t = item.InnerText.Trim();
                    if (t.Length > 0)
                        song.TrackArtist = t;
                }
            }
        }
        internal string ParseXiamiIDString(string linktext)
        {
            var start = linktext.LastIndexOf("/") + 1;
            var end = linktext.IndexOf("?");
            return end >= 0 ? linktext.Substring(start, end - start) : linktext.Substring(start);
        }
        internal uint ParseXiamiID(string linktext) => uint.Parse(ParseXiamiIDString(linktext));
        internal uint ParseXiamiID(HtmlNode linknode) => ParseXiamiID(linknode.GetAttributeValue("href", "/0"));
        #endregion
        #region 获取歌曲信息

        public IAsyncAction GetSongInfo(SongModel song, bool cover = true)
        {
            if (song.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Song {song.XiamiID}", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/song/{song.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode("/html/body/div[@id='page']");
                    List<Task> process = new List<Task>();
                    process.Add(Task.Run(() =>
                    {
                        if (song.RelatedLovers == null || cover)
                            song.RelatedLovers = new PageItemsCollection<UserModel>(ParseSongRelateUsers(body.SelectSingleNode(".//div[@id='song_fans_block']/div/ul")).ToList());
                    }, token));
                    process.Add(Task.Run(() =>
                    {
                        if (song.RelateHotSongs == null || cover)
                            song.RelateHotSongs = ParseSongRelateSongs(body.SelectSingleNode(".//div[@id='relate_song']/div/table")).ToList();
                    }, token));
                    process.Add(Task.Run(() =>
                    {
                        if (song.Tags == null || cover)
                            song.Tags = ParseTags(body.SelectSingleNode(".//div[@id='song_tags_block']/div")).ToList();
                    }, token));

                    var title = body.SelectSingleNode(".//h1");
                    if (song.Name == null || cover)
                        song.Name = title.FirstChild.InnerText;
                    var mva = title.SelectSingleNode("./a");
                    if ((mva != null) && (song.MV == null || cover))
                        song.MV = MVModel.GetNew(ParseXiamiIDString(mva.GetAttributeValue("href", "/0")));
                    if (song.Description == null || cover)
                        if (title.LastChild.Name == "span")
                            song.Description = title.LastChild.InnerText;
                    var loveop = body.SelectSingleNode(".//ul/li[1]");
                    song.IsLoved = loveop.GetAttributeValue("style", "") == "display:none";
                    if (loveop.ParentNode.ParentNode.ParentNode.InnerText.IndexOf("单曲下架") != -1) song.Available = false;
                    var detail = body.SelectSingleNode(".//table");
                    foreach (var item in detail.SelectNodes("./tr"))
                    {
                        switch (item.ChildNodes[1].InnerText)
                        {
                            case "所属专辑：":
                                var linknode = item.SelectSingleNode(".//a");
                                var id = uint.Parse(linknode.GetAttributeValue("href", "/album/0").Substring(7));
                                if ((song.Album == null) || cover)
                                {
                                    var album = song.Album ?? AlbumModel.GetNew(id);
                                    album.Name = linknode.InnerText;
                                    if (album.Art.Host == "")
                                    {
                                        var art = detail.ParentNode.SelectSingleNode(".//img").GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                                        album.Art = new Uri(art.Replace("_2", "_1"));
                                        album.ArtFull = new Uri(art.Replace("_2", ""));
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


                    await Task.WhenAll(process);
                    LogService.DebugWrite($"Finish Getting info of Song {song.XiamiID}", nameof(WebApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }

        internal IEnumerable<UserModel> ParseSongRelateUsers(HtmlNode listnode)
        {
            foreach (var item in listnode.ChildNodes)
            {
                if (item.NodeType != HtmlNodeType.Element)
                    continue;
                var imga = item.SelectSingleNode("./a");
                var id = uint.Parse(imga.GetAttributeValue("name_card", "0"));
                UserModel user = UserModel.GetNew(id);
                if (user.Art.Host == "")
                {
                    var avatar = imga.FirstChild.GetAttributeValue("src", @"ms-appx:///Assets/Pictures/usr50.gif");
                    if (avatar == @"http://img.xiami.net/res/img/default/usr50.gif") avatar = @"ms-appx:///Assets/Pictures/usr50.gif";
                    user.Art = new Uri(avatar);
                    user.ArtFull = new Uri(avatar.Replace("_1", ""));
                }
                user.Name = item.SelectSingleNode("./p/a").InnerText;
                yield return user;
            }
        }
        internal IEnumerable<SongModel> ParseSongRelateSongs(HtmlNode listnode)
        {
            foreach (var item in listnode.ChildNodes)
            {
                if (item.NodeType != HtmlNodeType.Element)
                    continue;
                var id = uint.Parse(item.SelectSingleNode("./input").GetAttributeValue("value", "0"));
                SongModel song = SongModel.GetNew(id);
                var title = item.SelectSingleNode(".//a");
                song.Name = title.GetAttributeValue("title", "unknown");
                var track = title.InnerText.Trim();
                song.TrackArtist = track.Substring(song.Name.Length + 3).Trim();
                yield return song;
            }
        }
        internal IEnumerable<string> ParseTags(HtmlNode listnode)
        {
            foreach (var item in listnode.ChildNodes)
                if (item.NodeType == HtmlNodeType.Element)
                    yield return item.InnerText;
        }

        #endregion
        #region 获取专辑信息
        public IAsyncAction GetAlbumInfo(AlbumModel album, bool cover = true)
        {
            if (album.XiamiID == 0)
                throw new ArgumentException("AlbumModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Album {album.XiamiID}", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/album/{album.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode("/html/body/div[@id='page']");
                    List<Task> process = new List<Task>();
                    process.Add(Task.Run(() =>
                    {
                        var listnode = body.SelectSingleNode(".//table[@id='track_list']");
                        if (album.SongList == null || cover)
                            album.SongList = ParseAlbumSongs(listnode, album).ToList();
                        else
                            ParseAlbumSongs(listnode, album.SongList);
                    }, token));

                    var title = body.SelectSingleNode(".//h1");
                    if (album.Name == null || cover)
                        album.Name = title.FirstChild.InnerText;
                    if (title.LastChild.NodeType != HtmlNodeType.Element)
                        if (album.Description == null || cover)
                            album.Description = title.LastChild.InnerText;
                    var info = body.SelectSingleNode(".//div[@id='album_info']");
                    album.Rating = info.SelectSingleNode(".//em").InnerText;
                    var ratings = info.SelectNodes(".//ul/li");
                    for (int i = 0; i < ratings.Count; i++)
                        album.RatingDetail[i] = int.Parse(ratings[i].LastChild.InnerText);
                    var loveop = body.SelectSingleNode(".//ul[@class='acts_list']");
                    album.IsLoved = loveop.SelectSingleNode("./li[1]").GetAttributeValue("style", "") == "display:none";
                    var share = loveop.SelectSingleNode(".//em").InnerText;
                    album.ShareCount = int.Parse(share.Substring(1, share.Length - 2));
                    foreach (var item in info.SelectNodes(".//tr"))
                    {
                        var tds = item.SelectNodes("./td");
                        switch (tds[0].InnerText)
                        {
                            case "艺人：":
                                if (album.Artist == null)
                                    album.Artist = ArtistModel.GetNew(ParseXiamiID(tds[1].SelectSingleNode(".//a").GetAttributeValue("href", "/0")));
                                break;
                            case "语种：":
                                album.Language = tds[1].InnerText;
                                break;
                            case "唱片公司：":
                                album.Publisher = tds[1].InnerText;
                                break;
                            case "发行时间":
                                album.ReleaseDate = tds[1].InnerText;
                                break;
                            case "专辑类别":
                                album.Type = tds[1].InnerText;
                                break;
                            case "专辑风格":
                                album.Genre = tds[1].SelectNodes("./a").Select((node) =>
                                {
                                    var gen = GenreModel.GetNew(ParseXiamiID(node.GetAttributeValue("href", "/0")));
                                    gen.Name = node.InnerText;
                                    return gen;
                                }).ToList();
                                break;
                        }
                    }
                    if (album.Art.Host == "")
                    {
                        var art = body.SelectSingleNode(".//img");
                        album.Art = new Uri(art.GetAttributeValue("src", AlbumModel.SmallDefaultUri));
                        album.ArtFull = new Uri(art.ParentNode.GetAttributeValue("href", AlbumModel.LargeDefaultUri));
                    }
                    if (album.Introduction == null || cover)
                        album.Introduction = body.SelectSingleNode(".//span[@property='v:summary']")?.InnerText?.Replace("<br />", "");

                    await Task.WhenAll(process);
                    LogService.DebugWrite($"Finishi Getting info of Album {album.XiamiID}", nameof(WebApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }

        internal IEnumerable<SongModel> ParseAlbumSongs(HtmlNode listnode, AlbumModel album)
        {
            string disc = null;
            foreach (var item in listnode.SelectNodes("./tbody/tr"))
            {
                if (item.SelectSingleNode("./td/strong") != null)
                    disc = item.InnerText;
                else
                {
                    SongModel song = SongModel.GetNew(uint.Parse(item.SelectSingleNode(".//input").GetAttributeValue("value", "0")));
                    song.Album = album;
                    song.DiscID = disc;
                    song.TrackID = int.Parse(item.ChildNodes[3].InnerText);
                    ParsePlayListSong(song, item);
                    //var title = songitem.SelectSingleNode(".//a");
                    //if (song.Name == null)
                    //    song.Name = title.InnerText;
                    //if (song.TrackArtist == null)
                    //{
                    //    string t = title.NextSibling.InnerText.Trim();
                    //    if (t.Length > 0)
                    //        song.TrackArtist = t;
                    //}
                    yield return song;
                }
            }
        }
        internal void ParseAlbumSongs(HtmlNode listnode, IEnumerable<SongModel> former)
        {//TODO: 待测试
            var iter = former.GetEnumerator();
            string disc = null;
            foreach (var item in listnode.ChildNodes)
            {
                if (item.NodeType != HtmlNodeType.Element)
                    continue;
                if (item.Name == "strong")
                    disc = item.InnerText;
                else
                    foreach (var songitem in item.SelectSingleNode("./tbody").ChildNodes)
                    {
                        if (songitem.NodeType != HtmlNodeType.Element)
                            continue;
                        if (!iter.MoveNext())
                            throw new ArgumentException("歌曲列表不匹配");
                        if (songitem.SelectSingleNode(".//input").GetAttributeValue("value", "0") != iter.Current.XiamiID.ToString())
                            throw new ArgumentException("歌曲列表ID不匹配");
                        //TODO: 增加Available判断
                        var cur = iter.Current;
                        cur.DiscID = disc;
                        cur.TrackID = int.Parse(songitem.ChildNodes[3].InnerText);
                        ParsePlayListSong(cur, songitem);
                    }
            }
        }

        #endregion
        #region 获取艺术家信息

        public IAsyncAction GetArtistInfo(ArtistModel artist, bool cover = true)
        {
            if (artist.XiamiID == 0)
                throw new ArgumentException("ArtistModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Artist {artist.XiamiID}", nameof(WebApi));
                    
                    List<Task> process = new List<Task>();
                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/artist/{artist.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    //System.Diagnostics.Debugger.Break();
                    if (artist.IsXiamiMusician)
                    { ParseMusicianInfo(artist, doc.DocumentNode.SelectSingleNode("/html/body/div[@id='Glory']"), cover); return; }
                    else if (doc.DocumentNode.SelectSingleNode("//h1").SelectSingleNode("../i[@title='音乐人']") != null)
                    {
                        artist.IsXiamiMusician = true;
                        ParseMusicianInfo(artist, doc.DocumentNode.SelectSingleNode("/html/body/div[@id='Glory']"), cover); return;
                    }
                    var body = doc.DocumentNode.SelectSingleNode("/html/body/div[@id='page']");
                    process.Add(Task.Run(async () =>
                    {
                        var pgettask = HttpHelper.GetAsync($"http://www.xiami.com/artist/profile/id/{artist.XiamiID}");
                        token.Register(() => pgettask.Cancel());
                        var pcontent = await pgettask;
                        HtmlDocument pdoc = new HtmlDocument();
                        pdoc.LoadHtml(pcontent);
                        var pdiv = pdoc.DocumentNode.SelectSingleNode(".//div[@class='profile']");
                        pdiv.RemoveChild(pdiv.SelectSingleNode("./h3"));
                        artist.Profile = pdiv.InnerHtml;
                    }, token));
                    process.Add(Task.Run(() =>
                    {
                        var id = artist.XiamiID;
                        artist.HotSongs = new PageItemsCollection<SongModel>(20, ParseArtistTopSongs(
                            body.SelectSingleNode(".//div[@class='common_sec']/table")).ToList(),
                            async (page, ptoken) =>
                            {
#if DEBUG
                                System.Diagnostics.Debugger.Break();
#endif
                                var pgettask = HttpHelper.GetAsync($"http://www.xiami.com/artist/top/id/{id}/page/{page}");
                                ptoken.Register(() => pgettask.Cancel());
                                var pcontent = await pgettask;
                                HtmlDocument pdoc = new HtmlDocument();
                                pdoc.LoadHtml(pcontent);
                                var pbody = pdoc.DocumentNode.SelectSingleNode("/html/body/div[@id='page']");
                                return ParseArtistTopSongs(pbody.SelectSingleNode(".//table[@class]")).ToList();
                            });
                    }, token));
                    var title = body.SelectSingleNode(".//h1");
                    if (artist.Name == null || cover)
                        artist.Name = title.FirstChild.InnerText;
                    if (artist.Description == null || cover)
                        if (title.LastChild.Name == "span")
                            artist.Description = title.LastChild.InnerText;
                    var detail = body.SelectSingleNode(".//table");

                    foreach (var item in detail.SelectNodes("./tr"))
                    {
                        var tds = item.SelectNodes("./td");
                        switch (tds[0].InnerText)
                        {
                            case "地区：":
                                artist.Area = tds[1].InnerText;
                                break;
                            case "风格：":
                                artist.Genre = tds[1].SelectNodes("./a").Select((node) =>
                                  {
                                      var gen = GenreModel.GetNew(ParseXiamiID(node.GetAttributeValue("href", "/0")));
                                      gen.Name = node.InnerText;
                                      return gen;
                                  }).ToList();
                                break;
                        }
                    }
                    if (artist.Art == null || cover)
                    {
                        var image = body.SelectSingleNode(".//img");
                        artist.Art = new Uri(image.GetAttributeValue("src", ArtistModel.SmallDefaultUri));
                        artist.ArtFull = new Uri(image.GetAttributeValue("src", ArtistModel.LargeDefaultUri).Replace("_2", ""));
                    }

                    await Task.WhenAll(process);
                    LogService.DebugWrite($"Finish Getting info of Artist {artist.XiamiID}", nameof(WebApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw e;
                }
            });
        }
        internal IEnumerable<SongModel> ParseArtistTopSongs(HtmlNode listnode)
        {
            foreach (var item in listnode.SelectNodes(".//tr"))
            {
                SongModel song = SongModel.GetNew(uint.Parse(item.SelectSingleNode(".//input").GetAttributeValue("value", "0")));
                ParsePlayListSong(song, item);
                yield return song;
            }
        }

        internal void ParseMusicianInfo(ArtistModel artist, HtmlNode contentnode, bool cover = true)
        {
            System.Diagnostics.Debug.Write("尚未完成获取音乐人信息");
        }
        #endregion
        #region 获取推荐

        public IAsyncOperation<MainRecBatchModel> GetMainRecs()
        {
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of MainPage", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync("http://www.xiami.com/");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode(".//body");
                    MainRecBatchModel res = new MainRecBatchModel();
                    List<Task> process = new List<Task>();
                    process.Add(Task.Run(() =>
                    {
                        res.NewInAll = ParseNewInAllAlbums(body.SelectSingleNode(".//div[@data-content='1_0']")).ToList();
                    }));
                    process.Add(Task.Run(() => {
                        res.PromoteCovers = (from item in body.SelectSingleNode(".//div[@id='slider']").SelectNodes(".//a[@title]")
                                            select new MainCoverModel() {
                                                SliderCoverSource = new Uri(item.SelectSingleNode("./img").GetAttributeValue("src", "")),
                                                RedirectUri = new Uri(item.GetAttributeValue("href", "")),
                                                Title = item.GetAttributeValue("title","")
                                            }).ToList();
                    }));
                    process.Add(Task.Run(() => { //TODO:获取时间定制精选集
                        }));

                    await Task.WhenAll(process);
                    LogService.DebugWrite("Finish Getting Mainpage", nameof(WebApi));
                    return res;
                }
                catch (Exception e) {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        } 
        //获取新碟首发
        internal IEnumerable<AlbumModel> ParseNewInAllAlbums(HtmlNode listnode)
        {
            foreach(var albumnode in listnode.ChildNodes)
            {
                if (albumnode.NodeType != HtmlNodeType.Element)
                    continue;
                var nodes = albumnode.SelectNodes("./div");
                var imagenode = nodes[0];
                AlbumModel album = AlbumModel.GetNew(ParseXiamiID(imagenode.Element("a").GetAttributeValue("href", "/0")));
                if (album.Art.Host == "")
                {
                    var art = imagenode.SelectSingleNode("./img").GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                    album.Art = new Uri(art.Replace("_5", "_1"));
                    album.ArtFull = new Uri(art.Replace("_5", ""));
                }
                var infonodes = nodes[1].SelectNodes("./p");
                if (album.Name == null)
                    album.Name = infonodes[0].InnerText;
                if (album.Artist == null)
                {
                    
                    var alinks = infonodes[1].SelectNodes("./a");
                    album.Artist = ArtistModel.GetNew(ParseXiamiID(alinks[0].GetAttributeValue("href", "/0")));
                    album.Artist.Name = alinks[0].InnerText;
                    if (alinks.Count > 1)
                        if (alinks[1].GetAttributeValue("title", "") == "音乐人")
                            album.Artist.IsXiamiMusician = true;
                }
                yield return album;
            }
        }

        public IAsyncOperation<DailyRecBatch> GetDailyRecs()
        {
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get Daily Recommendation", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync("http://www.xiami.com/index/recommend?_" + ParamHelper.GetTimestamp());
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    var i1 = content.IndexOf("\"data\"") + 7;
                    var i2 = content.LastIndexOf("\"jumpurl\"") - 3;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(Regex.Unescape(content.Substring(i1,i2-i1)));
                    DailyRecBatch res = new DailyRecBatch();
                    res.RecCollectionCovers = doc.DocumentNode.SelectSingleNode(".//div[@class='image']").SelectNodes("./img")
                        .Select((node) => new Uri(node.GetAttributeValue("src", AlbumModel.SmallDefaultUri))).ToList();//图片质量为_1
                    res.RecAlbums = doc.DocumentNode.SelectSingleNode(".//div[@class='main']").SelectNodes("./div")
                        .Select((node) => {
                            var arec = new AlbumRecModel();
                            var items = node.SelectNodes("./div");
                            AlbumModel album = AlbumModel.GetNew(ParseXiamiID(items[0].SelectSingleNode(".//a").GetAttributeValue("href", "/0")));
                            arec.Target = album;
                            var image = items[0].SelectSingleNode("./img").GetAttributeValue("src", AlbumModel.SmallDefaultUri);
                            album.Art = new Uri(image);
                            album.ArtFull = new Uri(image.Replace("_5", ""));

                            var texts = items[1].SelectNodes("./p");
                            var ar = texts[2].SelectSingleNode("./a");
                            ArtistModel artist = ArtistModel.GetNew(ParseXiamiID(ar.GetAttributeValue("href", "/0")));
                            artist.Name = ar.InnerText;
                            album.Artist = artist;
                            album.Name = texts[1].InnerText;
                            arec.ReasonRaw = texts[0].InnerHtml;

                            return arec;
                        }).ToList();

                    LogService.DebugWrite("Finish Getting Daily Recommendation", nameof(WebApi));
                    return res;
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }

        #endregion
        #region 电台操作

        public IAsyncOperation<uint> GetRadioId(RadioType radiotype, uint oid)
        {
            if (radiotype == RadioType.UnKnown||oid==0)
                throw new ArgumentException("Radio参数设置错误");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get id of Radio type={radiotype} oid={oid}", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/radio/play/type/{(int)radiotype}/oid/{oid}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    var start = content.IndexOf("value=\"");
                    start += 7;
                    var end = content.IndexOf("\"", start);
                    LogService.DebugWrite($"Finish Getting id of Radio oid={oid}", nameof(WebApi));
                    return uint.Parse(content.Substring(start, end - start));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw e;
                }
            });
        }
        public IAsyncOperation<Tuple<RadioType,uint>> GetRadioType(uint radioid)
        {
            if (radioid == 0)
                throw new ArgumentException("RadioModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get type of Radio id={radioid}", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/radio/play/id/{radioid}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    var start = content.IndexOf("dataUrl=/");
                    var end = content.IndexOf("&", start);
                    var cases = content.Substring(start, end - start).Split('/');
                    var t = new Tuple<RadioType, uint>((RadioType)(Enum.Parse(typeof(RadioType),cases[4])), uint.Parse(cases[6]));
                    LogService.DebugWrite($"Finish Getting type of Radio id={radioid}", nameof(WebApi));
                    return t;
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }
        public IAsyncAction FreshRadio(RadioService radio)
        {
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Fresh Radio Songlist {radio.Radio.ToString()}", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/radio/xml/type/{(int)(radio.Radio.Type)}/id/{radio.Radio.OID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(content);
                    radio.Clear();
                    foreach (var item in doc.DocumentElement.SelectNodes(".//track"))
                    {
                        SongModel song = SongModel.GetNew(uint.Parse(item.ElementText("song_id")));
                        song.Name = item.Element("title").InnerText;
                        if (song.Album == null)
                        {
                            AlbumModel am = AlbumModel.GetNew(uint.Parse(item.ElementText("album_id")));
                            am.Name = item.ElementText("album_name");
                            if (am.Artist == null)
                            {
                                ArtistModel ar = ArtistModel.GetNew(uint.Parse(item.ElementText("artist_id")));
                                ar.Name = item.ElementText("artist");
                                am.Artist = ar;
                            }
                            var image = item.ElementText("pic");
                            am.Art = new Uri(image.Replace("_1", "_2"));
                            am.ArtFull = new Uri(image.Replace("_1", ""));
                            song.Album = am;
                            var encry = item.ElementText("location");
                            var decry = DataApi.ParseDownloadLink(int.Parse(encry[0].ToString()), encry.Substring(1));
                            song.MediaUri = new Uri(System.Net.WebUtility.UrlDecode(decry).Replace('^', '0'));
                            song.Duration = TimeSpan.FromSeconds(double.Parse(item.ElementText("length")) * 1000 + 1);
                        }
                        radio.Enqueue(song);
                    }
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }

        #endregion
        #region 获取用户信息

        public IAsyncAction GetUserInfo(UserModel user)
        {
            if (user.XiamiID == 0)
                throw new ArgumentException("UserModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of User {user.XiamiID}", nameof(WebApi));
                    //System.Diagnostics.Debugger.Break();

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/u/{user.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode(".//div[@id='profile_index']");
                    var title = body.SelectSingleNode("./div/div");
                    if (user.Name == null)
                        user.Name = title.SelectSingleNode(".//h1/a").InnerText;
                    if (title.SelectSingleNode(".//i") != null)
                        user.IsVIP = true;
                    var mainpaneldivs = body.SelectSingleNode(".//div[@class='proMain_left_inner']");
                    var radionode = mainpaneldivs.SelectSingleNode("./div[@id='p_radio']");
                    if (user.UserRadio == null)
                    {
                        UserRadioModel radio = RadioModel.GetFromUser(user);
                        var radioimg = radionode.SelectSingleNode(".//img").GetAttributeValue("src", UserRadioModel.LargeDefaultUri);
                        radio.Art = new Uri(radioimg);
                        radio.ArtFull = new Uri(radioimg.Replace("_1", ""));
                        radio.Description = radionode.SelectSingleNode(".//p[@class='des']").InnerText;
                        user.UserRadio = radio;
                    }
                    user.Description = mainpaneldivs.SelectSingleNode(".//p[@class='tweeting_full']").InnerHtml;
                    user.RecentTracks = new PageItemsCollection<ListenLogModel>(50,
                        ParseLogSong(mainpaneldivs.SelectSingleNode("./div[@id='p_tracks']")),
                        async (page, ptoken) =>
                        {
                            var pgettask = HttpHelper.GetAsync($"http://www.xiami.com/space/charts-recent/u/{user.XiamiID}/page/{page}");
                            ptoken.Register(() => pgettask.Cancel());
                            var pcontent = await pgettask;
                            HtmlDocument pdoc = new HtmlDocument();
                            pdoc.LoadHtml(pcontent);
                            var pbody = pdoc.DocumentNode.SelectSingleNode("/html/body/div[@id='page']");
                            return ParseLogSong(pbody.SelectSingleNode(".//table[@class]")).ToList();
                        });
                    var rightpaneldivs = body.SelectNodes(".//div[@class='proMain_side']/div");
                    if (user.Art.Host == "")
                    {
                        var art = rightpaneldivs[0].SelectSingleNode(".//img[last()]").GetAttributeValue("src", @"ms-appx:///Assets/Pictures/usr50.gif");
                        user.Art = new Uri(art.Replace("_3", "_2"));
                        user.ArtFull = new Uri(art.Replace("_3", ""));
                    }
                    var details = rightpaneldivs[1];
                    var dps = details.SelectNodes("./p");
                    user.PersonalDescription = dps[0].InnerText;
                    user.JoinDate = dps[1].InnerText;
                    var lever = details.SelectSingleNode("./div[2]/div");
                    user.Level = lever.SelectSingleNode("./a").InnerText;
                    string vcount = lever.NextSibling.InnerText;
                    user.VisitedCount = int.Parse(vcount.Remove(vcount.IndexOf("次访问")));
                    var counts = details.Element("ul").SelectNodes("./li/a/span");
                    user.FollowingCount = int.Parse(counts[0].InnerText);
                    user.FollowerCount = int.Parse(counts[1].InnerText);

                    //其他的PageLoad集合：收藏的歌曲、专辑、艺人

                    LogService.DebugWrite($"Finish Getting info of User {user.XiamiID}", nameof(WebApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }

        internal IEnumerable<ListenLogModel> ParseLogSong(HtmlNode listnode)
        {
            foreach (var tr in listnode.SelectNodes(".//tr"))
            {
                var input = tr.SelectSingleNode(".//input");
                SongModel song = SongModel.GetNew(uint.Parse(input.GetAttributeValue("value", "0")));
                song.Available = input.GetAttributeValue("checked", "") == "checked";
                var namenodes = tr.SelectNodes("./td[@class='song_name']/a");
                song.Name = namenodes[0].GetAttributeValue("title", null);
                song.TrackArtist = namenodes[1].GetAttributeValue("title", null);
                LogDevices device = LogDevices.Web;//TODO：分析记录设备
                yield return new ListenLogModel(song, device, tr.SelectSingleNode("./td[@class='track_time']").InnerText);
            }
        }

        #endregion
        #region 搜索
        public IAsyncOperation<SearchResultBase> SearchBrief(string keyword)
        {
            if (keyword == null || keyword?.Length == 0)
                throw new ArgumentException("搜索内容不能为空");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Search key {keyword} briefly", nameof(WebApi));
                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/ajax/search-index?key={System.Net.WebUtility.UrlEncode(keyword.Replace(' ','+'))}&_={ParamHelper.GetTimestamp()}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var categories = doc.DocumentNode.SelectNodes(".//tr");
                    List<ArtistModel> ams = null;
                    List<AlbumModel> abms = null;
                    List<SongModel> sms = null;
                    foreach (var item in categories)
                    {
                        var lis = item.Descendants("li");
                        switch(item.Descendant("h3")?.InnerText)
                        {
                            case "歌曲":
                                sms = lis.Select((node) =>
                                {
                                    var linknode = node.Element("a");
                                    var linkt = linknode.GetAttributeValue("href", "/0?");
                                    var song = SongModel.GetNew(ParseXiamiID(linkt));
                                    song.TrackArtist = linknode.LastChild.InnerHtml;
                                    song.NameHtml = linknode.InnerHtml.Substring(0, linknode.InnerHtml.IndexOf("- <span>"));
                                    //System.Diagnostics.Debugger.Break();
                                    return song;
                                }).ToList();
                                break;
                            case "专辑":
                                abms = lis.Select((node) =>
                                {
                                    var linknode = node.Element("a");
                                    var linkt = linknode.GetAttributeValue("href", "/0?");
                                    var album = AlbumModel.GetNew(ParseXiamiID(linkt));
                                    if (album.Art.Host == "")
                                    {
                                        var art = linknode.Descendant("img").GetAttributeValue("src", AlbumModel.LargeDefaultUri);
                                        album.Art = new Uri(art.Replace("_3", "_2"));
                                        album.ArtFull = new Uri(art.Replace("_3", ""));
                                    }
                                    var snode = linknode.Descendant("strong");
                                    album.NameHtml = snode.InnerHtml;
                                    album.ArtistHtml = snode.ParentNode.ChildNodes.Last().InnerHtml.Trim();
                                    return album;
                                }).ToList();
                                break;
                            case "艺人":
                                ams = lis.Select((node) =>
                                {
                                    var linknode = node.Element("a");
                                    var linkt = linknode.GetAttributeValue("href", "/0?");
                                    var artist = ArtistModel.GetNew(ParseXiamiID(linkt));
                                    artist.Art = new Uri(linknode.Descendant("img").GetAttributeValue("src", ArtistModel.LargeDefaultUri));
                                    artist.NameHtml = linknode.Descendant("strong").InnerHtml;
                                    return artist;
                                }).ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    SearchResultBase result = new SearchResultBase() { Songs = sms, Artists = ams, Albums = abms };
                    LogService.DebugWrite($"Finish Searching {keyword}", nameof(WebApi));
                    return result;
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }

        public IAsyncOperation<SearchResult> Search(string keyword)
        {
            if (keyword == null || keyword?.Length == 0)
                throw new ArgumentException("搜索内容不能为空");
            keyword = keyword.Replace(' ', '+');
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Search key {keyword}", nameof(WebApi));
                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/search?key={keyword}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    SearchResult res = new SearchResult();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var root = doc.DocumentNode.SelectSingleNode("html/body/div[@id='page']");
                    var results = root.SelectSingleNode(".//div[@class='search_result']").Elements("div").ToList();

                    var match = results[0];//最佳匹配内容
                    if (match.GetAttributeValue("class", "").Contains("top_box"))
                    {
                        results.Remove(match);
                        // TODO: 处理最佳匹配
                    }

                    var songs = results[0];
                    var songres = ParseSearchedSongTable(songs.Descendant("table"));
                    //System.Diagnostics.Debugger.Break();
                    if (songs.Element("span") != null)
                        res.Songs = new PageItemsCollection<SongModel>(songres,
                            async (page, ptoken) =>
                            {
                                var pgettask = HttpHelper.GetAsync($"http://www.xiami.com/search/song/page/{page}?key={keyword}");
                                ptoken.Register(() => pgettask.Cancel());
                                var pcontent = await pgettask;
                                if (CheckNeedLogin(pcontent)) return null; // 需要登录才能看5页以后的内容
                                var pdoc = new HtmlDocument();
                                pdoc.LoadHtml(pcontent);
                                return ParseSearchedSongTable(pdoc.DocumentNode.SelectSingleNode(".//table[@class='track_list']"));
                            });
                    else
                        res.Songs = songres;

                    var albums = results[1];
                    var albumres = albums.Descendant("ul").Elements("li").Select((node) => ParseSearchedAlbum(node));
                    //System.Diagnostics.Debugger.Break();
                    if (albums.Element("span") != null)
                        res.Albums = new PageItemsCollection<AlbumModel>(30, albumres,
                            async (page, ptoken) =>
                            {
                                var pgettask = HttpHelper.GetAsync($"http://www.xiami.com/search/album/page/{page}?key={keyword}");
                                ptoken.Register(() => pgettask.Cancel());
                                var pcontent = await pgettask;
                                if (CheckNeedLogin(pcontent)) return null;
                                var pdoc = new HtmlDocument();
                                pdoc.LoadHtml(pcontent);
                                return pdoc.DocumentNode.SelectSingleNode(".//ul[@class='clearfix']")
                                    .Elements("li").Select((node) => ParseSearchedAlbum(node));
                            });
                    else
                        res.Albums = albumres.ToList();

                    var artists = results[2];
                    var artistres = artists.Descendant("ul").Elements("li").Select((node) => ParseSearchedArtist(node));
                    //System.Diagnostics.Debugger.Break();
                    if (artists.Element("span") != null)
                        res.Artists = new PageItemsCollection<ArtistModel>(30, artistres,
                            async (page, ptoken) =>
                            {
                                var pgettask = HttpHelper.GetAsync($"http://www.xiami.com/search/artist/page/{page}?key={keyword}");
                                ptoken.Register(() => pgettask.Cancel());
                                var pcontent = await pgettask;
                                if (CheckNeedLogin(pcontent)) return null;
                                var pdoc = new HtmlDocument();
                                pdoc.LoadHtml(pcontent);
                                return pdoc.DocumentNode.SelectSingleNode(".//div[@class='artistBlock_list ']").Element("ul")
                                    .Elements("li").Select((node) => ParseSearchedArtist(node));
                            });
                    else
                        res.Artists = artistres.ToList();
                    if (results.Count > 3)
                    {
                        var collects = results[3];
                        //TODO: 完成collection的搜索部分
                    }

                    LogService.DebugWrite($"Finish Searching {keyword}", nameof(WebApi));
                    return res;
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
        }

        internal List<SongModel> ParseSearchedSongTable(HtmlNode table)
        {
            string temp = table.InnerHtml;
            foreach(Match m in Regex.Matches(temp, @"(<tbody\s)[\s\S]+?(/tbody>)"))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("</tbody>");
                sb.Append(m.Value);
                sb.Append("<tbody>");
                temp = temp.Replace(m.Value, sb.ToString());
            }
            table.InnerHtml = temp;
            List<SongModel> res = new List<SongModel>();
            foreach(var node in table.Elements("tbody"))
            {
                if (node.GetAttributeValue("class", "").Contains("same_song_group"))
                    res.AddRange(node.Elements("tr").Select((tr) =>
                    {
                        var song = ParseSearchedSong(tr);
                        song.DuplicateOf = res[res.Count - 1];
                        return song;
                    })); // 不全部返回的话会造成歌曲数目不够，PageItem加载时会产生Exception
                else
                    res.AddRange(node.Elements("tr").Select((tr) => ParseSearchedSong(tr)));
            };
            return res;
        }
        internal SongModel ParseSearchedSong(HtmlNode tr)
        {
            //System.Diagnostics.Debugger.Break();
            var tds = tr.Elements("td").ToList();
            var checkbox = tds[0].Descendant("input");
            SongModel song = SongModel.GetNew(uint.Parse(checkbox.GetAttributeValue("value", "0")));
            song.Available = checkbox.GetAttributeValue("checked", "") == "checked";
            var links = tds[1].SelectNodes("./a[@target='_blank']");
            song.NameHtml = links[0].InnerHtml;
            if (links.Count > 1) song.MV = MVModel.GetNew(ParseXiamiIDString(links[1].GetAttributeValue("href", "/0")));
            var anode = tds[3].Element("a");
            var album = AlbumModel.GetNew(ParseXiamiID(anode.GetAttributeValue("href", "/0")));
            album.NameHtml = anode.InnerHtml.Replace("《", "").Replace("》", "");
            var arnode = tds[2].Element("a");
            album.Artist = ArtistModel.GetNew(ParseXiamiID(arnode.GetAttributeValue("href", "/0")));
            album.Artist.NameHtml = arnode.InnerHtml;
            song.Album = album;
            return song;
        }
        internal AlbumModel ParseSearchedAlbum(HtmlNode li)
        {
            //System.Diagnostics.Debugger.Break();
            var ips = li.SelectNodes("./div/p");
            var albumlink = ips[1].Element("a");
            var artistlink = ips[1].Elements("a").Last();
            AlbumModel album = AlbumModel.GetNew(ParseXiamiID(albumlink));
            album.Name = albumlink.GetAttributeValue("title", default(string));
            album.NameHtml = albumlink.InnerHtml;
            if (album.Art.Host == "")
            {
                var art = ips[0].Descendant("img").GetAttributeValue("src", AlbumModel.LargeDefaultUri);
                album.Art = new Uri(art.Replace("_1", "_2"));
                album.ArtFull = new Uri(art.Replace("_1", ""));
            }
            try
            {
                album.Artist = ArtistModel.GetNew(ParseXiamiID(artistlink));
                album.Artist.Name = artistlink.GetAttributeValue("title", default(string));
                album.Artist.NameHtml = artistlink.InnerHtml;
            }
            catch (Exception e) { LogService.ErrorWrite(e); album.Artist = ArtistModel.Null; }
            album.Rating = ips[2].Descendant("em").InnerText;
            album.ReleaseDate = ips[3].InnerText;
            return album;
        }
        internal ArtistModel ParseSearchedArtist(HtmlNode li)
        {
            var ips = li.SelectNodes("./div/p");
            var artistlink = ips[1].Element("a");
            //System.Diagnostics.Debugger.Break();
            ArtistModel artist = ArtistModel.GetNew(ParseXiamiID(artistlink));
            if (artist.Art.Host == "")
            {
                var art = ips[0].Descendant("img").GetAttributeValue("src", AlbumModel.LargeDefaultUri);
                artist.Art = new Uri(art.Replace("_1", "_2"));
                artist.ArtFull = new Uri(art.Replace("_1", ""));
            }
            artist.NameHtml = artistlink.InnerHtml;
            if (ips.Count > 1)
                artist.Area = ips[1].Element("span")?.InnerText;
            return artist;
        }
        internal CollectionModel ParseSearchedCollection(HtmlNode li)
        {
            throw new NotImplementedException();
        }
        #endregion
        
    }
}

/*
            if (song.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Song {song.XiamiID}", nameof(WebApi));
                    //System.Diagnostics.Debugger.Break();
                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/song/{song.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    
                    LogService.DebugWrite($"Finish Getting info of Song {song.XiamiID}", nameof(WebApi));
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(WebApi));
                    throw e;
                }
            });
*/
