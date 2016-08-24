﻿using HtmlAgilityPack;
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                    LogService.DebugWrite($"Get info of Song {song.XiamiID}", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/song/{song.XiamiID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode("/html/body/div[@id='page']");
                    List<Task> process = new List<Task>();
                    process.Add(Task.Run(() =>
                    {
                        if (song.RelatedLovers == null || cover)
                            song.RelatedLovers = ParseSongRelateUsers(body.SelectSingleNode(".//div[@id='song_fans_block']/div/ul")).ToList();
                    }));
                    process.Add(Task.Run(() =>
                    {
                        if (song.RelateHotSongs == null || cover)
                            song.RelateHotSongs = ParseSongRelateSongs(body.SelectSingleNode(".//div[@id='relate_song']/div/table")).ToList();
                    }));
                    process.Add(Task.Run(() =>
                    {
                        if (song.Tags == null || cover)
                            song.Tags = ParseTags(body.SelectSingleNode(".//div[@id='song_tags_block']/div")).ToList();
                    }));

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
                        switch (item.ChildNodes[1].InnerText)
                        {
                            case "所属专辑：":
                                var linknode = item.SelectSingleNode(".//a");
                                var id = uint.Parse(linknode.GetAttributeValue("href", "/album/0").Substring(7));
                                if ((song.Album == null) || cover)
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


                    await Task.WhenAll(process);
                    LogService.DebugWrite($"Finishi Getting info of Song {song.XiamiID}", nameof(WebApi));
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
                if (user.AvatarUri.Host == "")
                {
                    var avatar = imga.FirstChild.GetAttributeValue("src", @"ms-appx:///Assets/Pictures/usr50.gif");
                    if (avatar == @"http://img.xiami.net/res/img/default/usr50.gif") avatar = @"ms-appx:///Assets/Pictures/usr50.gif";
                    user.AvatarUri = new Uri(avatar);
                    user.AvatarFullUri = new Uri(avatar.Replace("_1", ""));
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

        public IAsyncAction GetAlbumInfo(AlbumModel album, bool cover = false)
        {
            if (album.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get info of Album {album.XiamiID}", nameof(WebApi));

                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/album/{album.XiamiID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode("/html/body/div[@id='page']");
                    List<Task> process = new List<Task>();
                    process.Add(Task.Run(() => 
                    {
                        var listnode = body.SelectSingleNode(".//div[@class='chapter mgt10']");
                        if (album.SongList == null || cover)
                            album.SongList = ParseAlbumSongs(listnode, album).ToList();
                        else
                            ParseAlbumSongs(listnode, album.SongList);
                    }));

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
                        switch (item.ChildNodes[1].InnerText)
                        {
                            case "艺人：":
                                if (album.Artist == null)
                                {
                                    var linknode = item.SelectSingleNode(".//a");
                                    var id = linknode.GetAttributeValue("href", "/0");
                                    album.Artist = ArtistModel.GetNew(uint.Parse(id.Substring(id.LastIndexOf('/') + 1)));
                                }
                                break;
                            case "语种：":
                                album.Language = item.ChildNodes[3].InnerText;
                                break;
                            case "唱片公司：":
                                album.Publisher = item.SelectSingleNode(".//a").InnerText;
                                break;
                            case "发行时间":
                                album.ReleaseDate = item.ChildNodes[3].InnerText;
                                break;
                            case "专辑类别":
                                album.Type = item.ChildNodes[3].InnerText;
                                break;
                        }
                    }
                    if (album.AlbumArtUri.Host == "")
                    {
                        var art = body.SelectSingleNode(".//img");
                        album.AlbumArtUri = new Uri(art.GetAttributeValue("src", "ms-appx:///Assets/Pictures/cd100.gif"));
                        album.AlbumArtFullUri = new Uri(art.ParentNode.GetAttributeValue("href", "ms-appx:///Assets/Pictures/cd500.gif"));
                    }
                    if (album.Introduction == null || cover)
                        album.Introduction = body.SelectSingleNode(".//span[@property='v:summary']").InnerText.Replace("<br />","");

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
                        SongModel song = SongModel.GetNew(uint.Parse(songitem.ChildNodes[1].FirstChild.GetAttributeValue("value", "0")));
                        song.Album = album;
                        song.DiscID = disc;
                        song.TrackID = int.Parse(songitem.ChildNodes[3].InnerText);
                        var title = songitem.SelectSingleNode(".//a");
                        if (song.Name == null)
                            song.Name = title.InnerText;
                        if (song.TrackArtist == null)
                        {
                            string t = title.NextSibling.InnerText.Trim();
                            if (t.Length > 0)
                                song.TrackArtist = t;
                        }
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
                        if (songitem.ChildNodes[1].FirstChild.GetAttributeValue("value", "0") != iter.Current.XiamiID.ToString())
                            throw new ArgumentException("歌曲列表ID不匹配");
                        var cur = iter.Current;
                        cur.DiscID = disc;
                        cur.TrackID = int.Parse(songitem.ChildNodes[3].InnerText);
                        if (cur.Name == null)
                            cur.Name = songitem.ChildNodes[3].ChildNodes[1].InnerText;
                    }
            }
        }

        public IAsyncAction GetArtistInfo(ArtistModel artist, bool cover = false)
        {
            throw new NotImplementedException();
        }
    }
}