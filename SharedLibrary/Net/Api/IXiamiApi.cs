using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace JacobC.Xiami.Net
{
    /* 测试对象：
     * Song：1770000099，1776283116，1775466136
     * Album：2100370674，422726，1585455615，1283296670，225281341，1127769596
     * Artist：3110
     */
    public interface IXiamiApi
    {
        IAsyncAction GetSongInfo(SongModel song, bool cover);
        IAsyncAction GetAlbumInfo(AlbumModel album, bool cover);
        IAsyncAction GetArtistInfo(ArtistModel artist, bool cover);
    }
}
