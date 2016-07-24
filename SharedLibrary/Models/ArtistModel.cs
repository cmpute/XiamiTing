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
    public class ArtistModel : SafeBindableBase
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
            ArtistModel album = null;
            if (!(_dict?.TryGetValue(XiamiID, out album) ?? false))
            {
                album = new ArtistModel() { ArtistID = XiamiID };
                _dict?.Add(XiamiID, album);
            }
            return album;
        }
        private ArtistModel() { }

        #region Playback Needed

        string _Name = default(string);
        /// <summary>
        /// 获取或设置艺术家的名字
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get { return _Name; } set { Set(ref _Name, value); } }

        string _AliasName = default(string);
        /// <summary>
        /// 获取或设置艺人的别名、英文名
        /// </summary>
        public string AliasName { get { return _AliasName; } set { Set(ref _AliasName, value); } }


        #endregion

        /// <summary>
        /// 获取或设置艺人的虾米ID
        /// </summary>
        public uint ArtistID { get; set; } = 0;

    }
}
