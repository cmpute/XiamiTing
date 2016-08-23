using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 精选集推荐
    /// </summary>
    public class CollectionRecommendationModel
    {
        public IList<CollectionModel> Topic;
        public IList<CollectionModel> XiaXiaoMi;
        public IList<ZoneModel> HotZones;
    }
}
