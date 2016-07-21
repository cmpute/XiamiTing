using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 虾米Windows桌面端使用的Api集（实际上桌面程序也大量使用了Web的api）
    /// </summary>
    public class WindowsDesktopApi : IXiamiInterface
    {
        /*TODO:
         * 未登录版本
         * av版本号可以没有，uid也可以没有 
         * /app/xiating/collect-recommend?av=XMusic_2.0.2.1618&uid=0 推荐精选集
         * /app/xiating/home-hot2?home_type=1&av=XMusic_2.0.2.1618&uid=0 热门专辑+热门艺人
         * /app/xiating/home-hot2?home_type=2&av=XMusic_2.0.2.1618&uid=0 推荐专辑+推荐艺人
         * /app/xiating/hot-music?av=XMusic_2.0.2.1618&uid=0 虾米音乐榜
         * 
         * 以下内容为api页面调用
         * /pc/下存放桌面版有关的样式和js文件
         * /api?api_key=..&api_sig=..&call_id=..&method=...&type=...&ver=...&page=.. api目录，返回json 其中如果method是Search.hotWords则是获取热词
         * 
         * 登录
         * /api/oauth2/token?grant_type=password&client_id=....&username=...&password=密码加密了  ，返回json，含有token
         * 
         * 登录以后
         * 
         * 登录以后/app/的页面uid=用户uid
         * 登录以后可以使用的api的method有Library.getLibrary, Library.getSongs（收藏的曲目）, Library.getCollects, Members.token， Search.autocomplete2, limit控制一页多少个，page可以控制页数
         */

        private WindowsDesktopApi() { }
        static WindowsDesktopApi _instance;
        /// <summary>
        /// 获取<see cref="WindowsDesktopApi"/>的唯一实例
        /// </summary>
        public static WindowsDesktopApi Instance
        {
            get
            {
                if (_instance == null) _instance = new WindowsDesktopApi();
                return _instance;
            }
        }

        /// <summary>
        /// 通过SongId获取歌曲的信息（不含取媒体地址）
        /// </summary>
        public async void GetSongInfo(SongModel song)
        {
            HttpClient hc = new HttpClient();
            var response = await hc.GetAsync(new Uri($"http://www.xiami.com/app/xiating/song?id={song.XiamiID}"));
            var content = await response.Content.ReadAsStringAsync();
            var maincontent = content.Substring(content.IndexOf("</style>"));
            song.Title = Regex.Match(maincontent, "*").Value;
        }
    }
}
