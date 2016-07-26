using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using Windows.Web.Http;
using Windows.Foundation;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 虾米Web端使用的Api集
    /// </summary>
    public class WebApi : IXiamiApi
    {
        /* TODO:
         * 
         */
        private WebApi() { }
        static WebApi _instance;
        /// <summary>
        /// 获取<see cref="WebApi"/>的唯一实例
        /// </summary>
        public static WebApi Instance
        {
            get
            {
                if (_instance == null) _instance = new WebApi();
                return _instance;
            }
        }

        public IAsyncAction GetSongInfo(SongModel song, bool cover = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction GetAlbumInfo(AlbumModel album, bool cover = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction GetArtistInfo(ArtistModel artist, bool cover = false)
        {
            throw new NotImplementedException();
        }
    }
}