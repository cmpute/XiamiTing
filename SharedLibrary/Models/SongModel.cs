using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Models
{
    public class SongModel
    {
        public int XiamiID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public Uri MediaUri { get; set; }
        public Uri AlbumArtUri { get; set; }
    }
}
