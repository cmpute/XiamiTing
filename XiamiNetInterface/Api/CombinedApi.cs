using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JacobC.Xiami.Models;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    public class CombinedApi : IXiamiApi
    {
        private CombinedApi() { }
        static CombinedApi _instance;
        /// <summary>
        /// 获取<see cref="CombinedApi"/>的唯一实例
        /// </summary>
        public static CombinedApi Instance
        {
            get
            {
                if (_instance == null) _instance = new CombinedApi();
                return _instance;
            }
        }

        public IAsyncAction GetAlbumInfo(AlbumModel album, bool cover = false)
        {
            return Run(async (c) =>
            {
                await WebApi.Instance.GetAlbumInfo(album, cover);
            });
        }

        public IAsyncAction GetArtistInfo(ArtistModel artist, bool cover = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction GetSongInfo(SongModel song, bool cover = false)
        {
            return Run(async (c) =>
            {
                await WapApi.Instance.GetSongInfo(song, cover);
                await WebApi.Instance.GetSongInfo(song, cover);
            });
        }
    }
}
