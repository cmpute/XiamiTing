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
        public PageItemsCollection<SongModel> Songs;
        public PageItemsCollection<AlbumModel> Albums;
        public PageItemsCollection<ArtistModel> Artist;
    }
    /// <summary>
    /// 
    /// </summary>
    public class SearchResult : SearchResultBase
    {
        
    }
}
