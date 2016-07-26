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
     * Album：2100370674，422726
     * Artist：3110
     */
    public interface IXiamiApi
    {
        IAsyncAction GetSongInfo(SongModel song, bool cover = false);
        IAsyncAction GetAlbumInfo(AlbumModel album, bool cover = false);
        IAsyncAction GetArtistInfo(ArtistModel artist, bool cover = false);
    }
}
