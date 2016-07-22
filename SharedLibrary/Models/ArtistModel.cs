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
