using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 推荐的基本模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RecommendationModel<T> where T : XiamiModelBase
    {
        /// <summary>
        /// 推荐的内容
        /// </summary>
        public T Target { get; set; }

        /// <summary>
        /// 推荐的原因（为未转义的html）
        /// </summary>
        public string ReasonRaw { get; set; }
    }
    /// <summary>
    /// 用户推荐的模型
    /// </summary>
    public class UserRecommendationModel : RecommendationModel<SongModel>
    {
        /// <summary>
        /// 推荐者
        /// </summary>
        public UserModel Nominator { get; set; }
    }
    /// <summary>
    /// 滑动封面的推荐
    /// </summary>
    public class MainCoverModel
    {
        public Uri SliderCoverSource { get; set; }
        public string Title { get; set; }
        public Uri RedirectUri { get; set; }
        public override string ToString()
        {
            return Title;
        }
    }
}
