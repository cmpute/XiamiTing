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
    public class ArtistModel : LovableModelBase
    {
        static Dictionary<uint, ArtistModel> _dict;
        static ArtistModel()
        {
            if (SettingsService.Instance.CacheItemsInDict)
                _dict = new Dictionary<uint, ArtistModel>();
        }
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

        Uri _ArtistAvatarUri = new Uri(@"ms-appx:///Assets/Pictures/cd100.gif");
        /// <summary>
        /// 获取或设置艺人头像的链接
        /// </summary>
        public Uri ArtistAvatarUri
        {
            get { return _ArtistAvatarUri; }
            set
            {
                if (_ArtistAvatarUri?.ToString() != value?.ToString())
                    Set(ref _ArtistAvatarUri, value);
            }
        }

        Uri _ArtistAvatarFullUri = new Uri(@"ms-appx:///Assets/Pictures/cd500.gif");
        /// <summary>
        /// 获取或设置艺人高清头像的链接
        /// </summary>
        public Uri ArtistAvatarFullUri
        {
            get { return _ArtistAvatarFullUri; }
            set
            {
                if (_ArtistAvatarFullUri?.ToString() != value?.ToString())
                    Set(ref _ArtistAvatarFullUri, value);
            }
        }

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


        /// <summary>
        /// 获取或设置艺人的虾米ID
        /// </summary>
        public uint XiamiID { get; set; } = 0;

        public override string ToString()
        {
            return $@"歌手：{Name} ID:{XiamiID}
地区：{Area} 别称：{AliasName}";
        }

    }
}
