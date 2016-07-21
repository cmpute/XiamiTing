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
    public class WebApi : IXiamiInterface
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

        /// <summary>
        /// 通过SongId获取歌曲的信息（不含取媒体地址）
        /// </summary>
        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> GetSongInfo(SongModel song)
        {
            HttpClient hc = new HttpClient();
            return hc.GetAsync(new Uri($"http://www.xiami.com/song/{song.XiamiID}"));
        }
    }
}