using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 读取通过Xml和Json传递的信息的类
    /// </summary>
    public static class DataApi
    {
        public static IAsyncAction GetSongBasicInfo(SongModel song)
        {
            if (song.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get basic info of Song {song.XiamiID}", nameof(DataApi));

                    var gettask = HttpHelper.GetAsync($"http://www.xiami.com/song/playlist?id={song.XiamiID}");
                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    //TODO: 完成剩下的部分
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(DataApi));
                    throw e;
                }
            });
        }

        internal static string ParseDownloadLink(int row, string locationstring)
        {
            int len = locationstring.Length;
            var cols = len / row;
            var rows_ex = len % row;
            List<string> matrix = new List<string>();
            for (int r = 0; r < row; r++)
            {
                var rlen = r < rows_ex ? cols + 1 : cols;
                matrix.Add(locationstring.Substring(0, rlen));
                locationstring = locationstring.Substring(rlen);
            }
            string res = "";
            for (int i = 0; i < len; i++)
                res += matrix[i % row][i / row];
            return res;
        }
        public static IAsyncOperation<string> GetDownloadLink(uint songID, bool isHQ)
        {
            return Run(async token =>
            {
                try
                {
                    LogService.DebugWrite($"Get link of Song {songID}", nameof(DataApi));

                    var gettask = HttpHelper.GetAsync(isHQ ? $"http://www.xiami.com/song/gethqsong/sid/{songID}" : $"http://www.xiami.com/song/playlist/id/{songID}");

                    token.Register(() => gettask.Cancel());
                    var content = await gettask;
                    var match = Regex.Match(content, isHQ? "(?<=location\":\").+?(?=\")" : "(?<=location>).+?(?=</location)");
                    if (!match.Success)
                    {
#if DEBUG
                        System.Diagnostics.Debugger.Break(); // TODO: 部分不支持播放的歌曲会在这报错
#endif
                        throw new Exception("获取下载地址出错");
                    }
                    var decry = ParseDownloadLink(int.Parse(match.Value[0].ToString()), match.Value.Substring(1));
                    return System.Net.WebUtility.UrlDecode(decry).Replace('^','0');
                }
                catch (Exception e)
                {
                    LogService.ErrorWrite(e, nameof(DataApi));
                    throw e;
                }
            });
        }
        public static IAsyncOperation<string> GetDownloadLink(SongModel song, bool isHQ)
        {
            if (song.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return GetDownloadLink(song.XiamiID,isHQ);
        }

        //http://www.xiami.com/count/playstat?vip%5Frole=0&type=0&song%5Fid=1771081923
        public static IAsyncAction PushPlayStateXiami(uint songID, ScrobbleState state)
        {
            return Run(async token =>
            {
                await Task.CompletedTask;
                throw new NotImplementedException();
            });
        }
        public static IAsyncAction PushPlayStateXiami(SongModel song, ScrobbleState state)
        {
            if (song.XiamiID == 0)
                throw new ArgumentException("SongModel未设置ID");
            return PushPlayStateXiami(song.XiamiID, state);
        }

        public static IAsyncOperation<bool> CheckUserIsVip(string username)
        {
            //http://www.xiami.com/vip/role?user_id={userid}&_ksTS={timestamp}_{x}&callback=jsonp{x+1}
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 标识收集播放信息时的状态
    /// </summary>
    public enum ScrobbleState
    {
        Start = 0,
        Part = 1,
        Part2 = 2,
        End = 3
    }
}
