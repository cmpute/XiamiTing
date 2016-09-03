using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class GenreModel : XiamiModelBase<uint>
    {
        static Dictionary<uint, GenreModel> _dict = new Dictionary<uint, GenreModel>();

        /// <summary>
        /// 获取一个新的<see cref="GenreModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="xiamiID">标志<see cref="GenreModel"/>的虾米ID</param>
        /// <returns></returns>
        public static GenreModel GetNew(uint xiamiID)
        {
            GenreModel song = null;
            if (!(_dict?.TryGetValue(xiamiID, out song) ?? false))
            {
                song = new GenreModel() { XiamiID = xiamiID };
                _dict?.Add(xiamiID, song);
            }
            return song;
        }
        private GenreModel() { }

        /// <summary>
        /// 获取该风格是不是风格大类
        /// </summary>
        public bool IsGroup { get { return XiamiID < 25; } }

        PageItemsCollection<SongModel> _TypicalSongs = null;
        /// <summary>
        /// 获取或设置代表曲目列表
        /// </summary>
        public PageItemsCollection<SongModel> TypicalSongs { get { return _TypicalSongs; } set { Set(ref _TypicalSongs, value); } }

        PageItemsCollection<ArtistModel> _TypicalArtists = null;
        /// <summary>
        /// 获取或设置代表专辑属性
        /// </summary>
        public PageItemsCollection<ArtistModel> TypicalArtists { get { return _TypicalArtists; } set { Set(ref _TypicalArtists, value); } }

        PageItemsCollection<AlbumModel> _TypicalAlbums = default(PageItemsCollection<AlbumModel>);
        /// <summary>
        /// 获取或设置代表专辑列表
        /// </summary>
        public PageItemsCollection<AlbumModel> TypicalAlbums { get { return _TypicalAlbums; } set { Set(ref _TypicalAlbums, value); } }

        public new PageItemsCollection<CommentModel> Comments { set { } } //暂时没有开放评论功能

    }
}
