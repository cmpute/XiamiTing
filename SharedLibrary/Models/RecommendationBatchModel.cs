using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 用于获取主页推荐内容的模型
    /// </summary>
    public class MainRecBatchModel
    {
        /// <summary>
        /// 新碟首发
        /// </summary>
        public IList<AlbumModel> NewInAll { get; set; }
        /// <summary>
        /// 时间定制电台
        /// </summary>
        public IList<Services.RadioService> GenreMusic { get; set; }
        /// <summary>
        /// 滑动封面
        /// </summary>
        public IList<MainCoverModel> PromoteCovers { get; set; }
    }

    /// <summary>
    /// 猜你喜欢
    /// </summary>
    public class DailyRecBatch
    {
        public IList<AlbumRecModel> RecAlbums;
        public IList<Uri> RecCollectionCovers;
    }
    //便于绑定用
    public class AlbumRecModel : RecommendationModel<AlbumModel> { }
}
