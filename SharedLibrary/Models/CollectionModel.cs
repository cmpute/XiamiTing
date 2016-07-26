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
    public class CollectionModel : LovableModelBase
    {

        static Dictionary<uint, CollectionModel> _dict;
        static CollectionModel()
        {
            if (SettingsService.Instance.CacheItemsInDict)
                _dict = new Dictionary<uint, CollectionModel>();
        }
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
        private CollectionModel() { }

        IEnumerable<SongModel> _SongList = default(IEnumerable<SongModel>);
        /// <summary>
        /// 获取或设置精选集的歌曲列表
        /// </summary>
        public IEnumerable<SongModel> SongList { get { return _SongList; } set { Set(ref _SongList, value); } }

    }
}
