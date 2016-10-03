using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 搜索结果的基类
    /// </summary>
    public class SearchResultBase
    {
        public IList<SongModel> Songs;
        public IList<AlbumModel> Albums;
        public IList<ArtistModel> Artists;
    }
    /// <summary>
    /// 完整搜索结果，IList的实现会实现IIncrementalLoading
    /// </summary>
    public class SearchResult : SearchResultBase
    {
        public XiamiModelBase BestMatch;
        public IList<CollectionModel> Collections;
    }
}
