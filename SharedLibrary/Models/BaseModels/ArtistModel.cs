using JacobC.Xiami.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 艺术家（歌手、编曲家等）的MVVM模型
    /// </summary>
    [DataContract]
    public class ArtistModel : LovableModelBase<uint>, ICovered
    {
        public const string SmallDefaultUri = @"ms-appx:///Assets/Pictures/usr100.gif";
        public const string LargeDefaultUri = @"ms-appx:///Assets/Pictures/usr100.gif";

        public static ArtistModel Null = new ArtistModel() { Name = "未知歌手", NameHtml = "未知歌手" };
        static Dictionary<uint, ArtistModel> _dict = new Dictionary<uint, ArtistModel>();
        /// <summary>
        /// 获取一个新的<see cref="ArtistModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="XiamiID">标志<see cref="ArtistModel"/>的虾米ID</param>
        /// <returns></returns>
        public static ArtistModel GetNew(uint XiamiID)
        {
            ArtistModel artist = null;
            if (!(_dict?.TryGetValue(XiamiID, out artist) ?? false))
            {
                artist = new ArtistModel() { XiamiID = XiamiID };
                _dict?.Add(XiamiID, artist);
            }
            return artist;
        }
        private ArtistModel() { }

        string _AliasName = default(string);
        /// <summary>
        /// 获取或设置艺人的别名、英文名
        /// </summary>
        public string AliasName { get { return _AliasName; } set { Set(ref _AliasName, value); } }

        string _Area = default(string);
        /// <summary>
        /// 获取或设置艺人所属地区
        /// </summary>
        public string Area { get { return _Area; } set { Set(ref _Area, value); } }

        string _Profile = default(string);
        /// <summary>
        /// 获取或设置艺术家简介属性(Html格式)
        /// </summary>
        public string Profile { get { return _Profile; } set { Set(ref _Profile, value); } }

        bool _IsXiamiMusician = false;
        /// <summary>
        /// 获取或设置是否是虾米音乐人
        /// </summary>
        public bool IsXiamiMusician { get { return _IsXiamiMusician; } set { Set(ref _IsXiamiMusician, value); } }


        #region ICovered Members
        Uri _ArtistAvatarUri = new Uri(SmallDefaultUri);
        /// <summary>
        /// 获取或设置艺人头像的链接
        /// </summary>
        public Uri Art
        {
            get { return _ArtistAvatarUri; }
            set
            {
                if (_ArtistAvatarUri?.ToString() != value?.ToString())
                    Set(ref _ArtistAvatarUri, value);
            }
        }

        Uri _ArtistAvatarFullUri = new Uri(LargeDefaultUri);
        /// <summary>
        /// 获取或设置艺人高清头像的链接
        /// </summary>
        public Uri ArtFull
        {
            get { return _ArtistAvatarFullUri; }
            set
            {
                if (_ArtistAvatarFullUri?.ToString() != value?.ToString())
                    Set(ref _ArtistAvatarFullUri, value);
            }
        }

        /// <summary>
        /// 获取指定大小的封面地址
        /// </summary>
        /// <param name="size">封面大小对应的数字
        /// </param>
        public Uri GetArtWithSize(int sizecode)
        {
            var origin = ArtFull.ToString();
            return new Uri(origin.Insert(origin.LastIndexOf('.'), "_" + (sizecode == 0 ? "" : sizecode.ToString())));
        }
        #endregion

        PageItemsCollection<SongModel> _HotSongs = null;
        /// <summary>
        /// 获取或设置艺人热门歌曲属性
        /// </summary>
        public PageItemsCollection<SongModel> HotSongs { get { return _HotSongs; } set { Set(ref _HotSongs, value); } }

        PageItemsCollection<AlbumModel> _Albums = default(PageItemsCollection<AlbumModel>);
        /// <summary>
        /// 获取或设置艺人所属的专辑
        /// </summary>
        public PageItemsCollection<AlbumModel> Albums { get { return _Albums; } set { Set(ref _Albums, value); } }


        IList<GenreModel> _Genre = default(IList<GenreModel>);
        /// <summary>
        /// 获取或设置艺人的风格
        /// </summary>
        public IList<GenreModel> Genre { get { return _Genre; } set { Set(ref _Genre, value); } }

        public override string ToString()
        {
            return $@"歌手：{Name} ID:{XiamiID}
地区：{Area} 别称：{AliasName}";
        }

    }
}
