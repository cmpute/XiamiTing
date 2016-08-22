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

        //专辑信息比较复杂，只考虑Web
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

        //TODO: 对比获取的歌曲信息，减少重复获取开支，尤其是几个IEnumerable。可以考虑WebApi的cover均为true
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
