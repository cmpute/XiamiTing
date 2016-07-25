using HtmlAgilityPack;
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// Windows桌面客户端使用的Api
    /// </summary>
    public class WindowsDesktopApi : IXiamiApi
    {
        //TODO: 需要增加cookieContainer，av=4.5
        //TODO: 需考虑如何将歌曲分成Disc1，2，3
        //TODO: 需考虑PageLoadCollection是存SongModel还是SongViewModel
    }
}
