using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 用于获取推荐内容的模型
    /// </summary>
    public class RecommendationModel
    {
        /// <summary>
        /// 每日推荐/猜你喜欢
        /// </summary>
        public IList<AlbumModel> Daily { get; set; }
        public IList<AlbumModel> NewInAll { get; set; }
        public IList<AlbumModel> NewInChinese { get; set; }
        public IList<AlbumModel> NewInEnglish { get; set; }
        public IList<AlbumModel> NewInJapanese { get; set; }
        public IList<AlbumModel> NewInKorean { get; set; }
        public IList<Services.RadioService> GenreMusic { get; set; }
        /// <summary>
        /// 大虾推荐
        /// </summary>
        public IList<UserRecommendationModel> UserRecommendations { get; set; }
        /// <summary>
        /// 今日音乐人
        /// </summary>
        public IList<UserRecommendationModel> MusicianRecommendations { get; set; }
    }
    /// <summary>
    /// 用户推荐的模型
    /// </summary>
    public class UserRecommendationModel
    {
        /// <summary>
        /// 推荐者
        /// </summary>
        public UserModel Nominator { get; set; }

        //下面两个可以考虑改成CommentModel？

        /// <summary>
        /// 推荐内容
        /// </summary>
        public SongModel Recommendation { get; set; }
        /// <summary>
        /// 推荐原因
        /// </summary>
        public string Reason { get; set; }
    }
}
