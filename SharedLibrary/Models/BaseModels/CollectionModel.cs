using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 虾米精选集的Model
    /// </summary>
    public class CollectionModel : LovableModelBase<uint>, ICovered
    {

        static Dictionary<uint, CollectionModel> _dict = new Dictionary<uint, CollectionModel>();
        /// <summary>
        /// 获取一个新的<see cref="CollectionModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="xiamiID">标志<see cref="CollectionModel"/>的虾米ID</param>
        /// <returns></returns>
        public static CollectionModel GetNew(uint xiamiID)
        {
            CollectionModel collect = null;
            if (!(_dict?.TryGetValue(xiamiID, out collect) ?? false))
            {
                collect = new CollectionModel() { XiamiID = xiamiID };
                _dict?.Add(xiamiID, collect);
            }
            return collect;
        }

        #region ICovered Members
        Uri _CoverArtUri = new Uri(AlbumModel.SmallDefaultUri);
        /// <summary>
        /// 获取或设置精选集封面的链接
        /// </summary>
        public Uri Art
        {
            get { return _CoverArtUri; }
            set
            {
                if (_CoverArtUri?.ToString() != value?.ToString())
                    Set(ref _CoverArtUri, value);
            }
        }

        Uri _CoverArtFullUri = new Uri(AlbumModel.LargeDefaultUri);
        /// <summary>
        /// 获取或设置精选集原图的链接
        /// </summary>
        public Uri ArtFull
        {
            get { return _CoverArtFullUri; }
            set
            {
                if (_CoverArtFullUri?.ToString() != value?.ToString())
                    Set(ref _CoverArtFullUri, value);
            }
        }

        /// <summary>
        /// 获取指定大小的封面地址
        /// </summary>
        /// <param name="size">封面大小对应的数字
        /// 0:原图 1:100x100 2:55x55
        /// </param>
        public Uri GetArtWithSize(int sizecode)
        {

            var origin = ArtFull.ToString();
            return new Uri(origin.Insert(origin.LastIndexOf('.'), "_" + (sizecode == 0 ? "" : sizecode.ToString())));
        }
        #endregion

        private CollectionModel() { }

        IEnumerable<SongModel> _SongList = default(IEnumerable<SongModel>);
        /// <summary>
        /// 获取或设置精选集的歌曲列表
        /// </summary>
        public IEnumerable<SongModel> SongList { get { return _SongList; } set { Set(ref _SongList, value); } }


    }
}
