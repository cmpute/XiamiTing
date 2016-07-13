using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class SongModel
    {
        //Json序列化时需要传递的信息
        #region Playback Needed
        public int XiamiID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public Uri MediaUri { get; set; }
        public Uri AlbumArtUri { get; set; }
        #endregion

    }
}
