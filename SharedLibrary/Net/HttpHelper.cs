using JacobC.Xiami.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace JacobC.Xiami.Net
{
    //TODO: 针对具体的网络异常进行处理
    public static class HttpHelper
    {
        public static readonly Uri XiamiDomain = new Uri("http://www.xiami.com");

        #region Public Objects
        static HttpClientHandler _handler;
        /// <summary>
        /// 获取控制Http请求属性的<see cref=""/>
        /// </summary>
        /// <remarks>设置后可能需要重启应用生效</remarks>
        public static HttpClientHandler Handler
        {
            get
            {
                if (_handler == null)
                {
                    _handler = new HttpClientHandler();
                    _handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                    ReadCookies(_handler);
                }
                return _handler;
            }
        }

        static HttpClient _client;
        static object clientlocker = new object();
        public static HttpClient Client
        {
            get
            {
                lock (clientlocker)
                {
                    if (_client == null)
                    {
                        _client = new HttpClient(Handler);
                        _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                        //_client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                        _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, sdch");
                        _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.8");
                        _client.DefaultRequestHeaders.Referrer = new Uri("http://www.xiami.com/play?ids=/song/playlist/id/1/type/9");
                        //_client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");
                        _client.Timeout = TimeSpan.FromSeconds(5);
                    }
                }
                return _client;
            }
        }

        #endregion

        #region Cookies
        public static void SaveCookies(HttpClientHandler handler)
        {
            foreach (var item in handler.CookieContainer.GetCookies(XiamiDomain))
            {
                var c = item as Cookie;
                SettingsService.NetCookies.Write(c.Name, c);
            }
        }
        public static void SaveCookies() => SaveCookies(Handler);
        /// <summary>
        /// 读取Cookie以后删除设置中的Cookie
        /// </summary>
        /// <param name="handler"></param>
        public static void ReadCookies(HttpClientHandler handler, bool reset = true)
        {
            if (handler.CookieContainer == null)
                handler.CookieContainer = new CookieContainer();
            foreach (var item in SettingsService.NetCookies.Values)
            {
                var t = reset ? SettingsService.NetCookies.ReadAndReset<Cookie>(item.Key) :
                    SettingsService.NetCookies.Read<Cookie>(item.Key);
                if (t != null)
                {
                    //直接Deserialize会设置Port，导致多出"$Port"
                    var copy = new Cookie(t.Name, t.Value, t.Path, t.Domain)
                    {
                        Comment = t.Comment,
                        Expired = t.Expired,
                        Expires = t.Expires,
                        HttpOnly = t.HttpOnly,
                        Secure = t.Secure,
                    };
                    handler.CookieContainer.Add(XiamiDomain, copy);
                }
            }
        }
        public static void ClearCookies()
        {
            _handler = new HttpClientHandler();
            _client = new HttpClient(_handler);
        }
        public static void PrintCookies() => PrintCookies(XiamiDomain);
        public static void PrintCookies(Uri domain)
        {
            var container = Handler.CookieContainer;
            foreach (System.Net.Cookie item in container.GetCookies(domain))
            {
                LogService.DebugWrite(item.ToString(), "Cookie");
            }
        }
        #endregion

        #region Get/Post
        /// <summary>
        /// 发送Http请求
        /// </summary>
        /// <param name="uri">请求地址</param>
        /// <param name="method">HTTP方法</param>
        /// <param name="content">请求内容</param>
        /// <param name="headersOperation">对请求标头的操作</param>
        /// <returns>请求的Response内容，注意Dispose</returns>
        internal static IAsyncOperation<HttpResponseMessage> SendMessageInternal(
            string uri,
            HttpMethod method,
            HttpContent content,
            Action<HttpRequestHeaders> headersOperation,
            [CallerMemberName]string caller = "")
        {
            return Run(async (rtoken) =>
            {
                HttpRequestMessage msg = new HttpRequestMessage(method, uri);
                if (content != null)
                    msg.Content = content;
                headersOperation?.Invoke(msg.Headers);
                var request = Client.SendAsync(msg);
                rtoken.Register(() => request.AsAsyncOperation().Cancel());
                try
                {
                    var get = await request;
                    if (!get.IsSuccessStatusCode)
                        throw new ConnectException("在HttpRequest中出现错误", new HttpRequestException(get.StatusCode.ToString()));
                    else
                        return get;
                }
                catch (System.Runtime.InteropServices.COMException ce)
                {
                    throw new ConnectException($"在{caller}中出现错误", ce);
                }
                catch (Exception e)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
                    return null;
#else
                    throw new ConnectException("待处理异常", e);
#endif
                }
            });
        }

        public static IAsyncOperation<string> PostAsync(string uri, HttpContent content, Action<HttpRequestHeaders> headersOperation = null)
        {
            return Run(async token =>
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var postTask = SendMessageInternal(uri,HttpMethod.Post,content, headersOperation);
                token.Register(() => postTask.Cancel());
                using (var resp = await postTask)
                    return await resp.Content.ReadAsStringAsync();
            });
        }
        public static IAsyncOperation<string> PostAsync(string uri, string request, Action<HttpRequestHeaders> headersOperation = null)
        {
            using (var re = new StringContent(request))
                return PostAsync(uri, re, headersOperation);
        }
        public static IAsyncOperation<string> GetAsync(string uri, Action<HttpRequestHeaders> headersOperation = null)
        {
            return Run(async token =>
            {
                var gettask = SendMessageInternal(uri, HttpMethod.Get, null, headersOperation);
                token.Register(() => gettask.Cancel());
                using (var resp = await gettask)
                    return await resp.Content.ReadAsStringAsync();
            });
        }
        public static IAsyncOperation<Stream> GetAsyncAsStream(string uri)
        {
            return Run(async token =>
            {
                var gettask = SendMessageInternal(uri, HttpMethod.Get, null, null);
                token.Register(() => gettask.Cancel());
                using (var resp = await gettask)
                    return await resp.Content.ReadAsStreamAsync();
            });
        }
        //TODO: HTML解析的时候使用Stream进行解析

        /// <summary>
        /// 获得一个HTTP的GET请求的响应流并且写入到本地文件中
        /// </summary>
        /// <param name="uri">需要请求的Uri</param>
        /// <param name="targetFile">需要储存到的文件对象</param>
        public static async Task GetHttpStreamToStorageFileAsync(this Uri uri, StorageFile targetFile)
        {
            using (var content = await Client.GetAsync(uri))
            {
                content.EnsureSuccessStatusCode();
                using (var fileStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var stream = fileStream.AsStreamForWrite())
                        await content.Content.CopyToAsync(stream);
                }
            }
        }
#endregion
    }
}
