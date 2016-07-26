using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using Windows.Web.Http;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    LogService.DebugWrite($"Get info of Song {song.XiamiID}", "NetInterface");

                    var gettask = HttpHelper.GetAsync(new Uri($"http://www.xiami.com/song/{song.XiamiID}"));
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var body = doc.DocumentNode.SelectSingleNode("//div[@id='page']");
                    List<Task> process = new List<Task>();
                    process.Add(Task.Run(() =>
                    {
                        if (song.RelatedLovers == null || cover)
                            song.RelatedLovers = ParseSongRelateUsers(body.SelectSingleNode(".//div[@id='song_fans_block']/div/ul"));
                    }));
                    process.Add(Task.Run(() =>
                    {
                        if (song.RelateHotSongs == null || cover)
                            song.RelateHotSongs = ParseSongRelateSongs(body.SelectSingleNode(".//div[@id='relate_song']/div/table"));
                    }));
                    process.Add(Task.Run(() =>
                    {
                        if (song.Tags == null || cover)
                            song.Tags = ParseSongTags(body.SelectSingleNode(".//div[@id='song_tags_block']/div"));
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
                    

                    await Task.WhenAll(process);
                    LogService.DebugWrite($"Finishi Getting info of Song {song.XiamiID}", "NetInterface");
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, "NetInterface");
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
        internal IEnumerable<string> ParseSongTags(HtmlNode listnode)
        {
            foreach (var item in listnode.ChildNodes)
                if (item.NodeType == HtmlNodeType.Element)
                    yield return item.InnerText;
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