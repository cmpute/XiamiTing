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
    public class ArtistModel : BindableBase
    {
        #region Playback Needed
        string _Name = default(string);
        /// <summary>
        /// 获取或设置艺术家的名字
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get { return _Name; } set { Set(ref _Name, value); } }

        #endregion
    }
}
