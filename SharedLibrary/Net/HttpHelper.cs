using JacobC.Xiami.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
                var c = item as System.Net.Cookie;
                SettingsService.NetCookies.Write(c.Name, c);
            }
        }
        /// <summary>
        /// 读取Cookie以后删除设置中的Cookie
        /// </summary>
        /// <param name="handler"></param>
        public static void ReadCookies(HttpClientHandler handler, bool reset = true)
        {
            if (handler.CookieContainer == null)
                handler.CookieContainer = new System.Net.CookieContainer();
            foreach (var item in SettingsService.NetCookies.Values)
            {
                var t = reset ? SettingsService.NetCookies.ReadAndReset<Cookie>(item.Key) :
                    SettingsService.NetCookies.Read<Cookie>(item.Key);
                if (t != null) handler.CookieContainer.Add(XiamiDomain, t);
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
        public static IAsyncOperation<string> PostAsync(Uri uri, HttpContent content)
        {
            return Run(async token =>
            {
                //re.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                var postTask = Client.PostAsync(uri, content);
                token.Register(() => postTask.AsAsyncOperation().Cancel());
                try
                {
                    using (var get = await postTask)
                    {
                        if (!get.IsSuccessStatusCode)
                            throw new ConnectException("在HttpRequest中出现错误", new HttpRequestException(get.StatusCode.ToString()));
                        else
                            return await get.Content.ReadAsStringAsync();
                    }
                }
                catch (System.Runtime.InteropServices.COMException ce)
                {
                    throw new ConnectException($"在{nameof(GetAsync)}中出现错误", ce);
                }
                catch(Exception e)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new ConnectException("待处理异常",e);
                }
            });
        }
        public static IAsyncOperation<string> PostAsync(Uri uri, string request)
        {
            using (var re = new StringContent(request))
            {
                return PostAsync(uri, re);
            }
        }

        public static IAsyncOperation<string> GetAsync(Uri uri)
        {
            return Run(async token =>
            {
                try
                {
                    var t = Client.GetAsync(uri, token);
                    using (var get = await t)
                    {
                        if (!get.IsSuccessStatusCode)
                            throw new ConnectException("在HttpRequest中出现错误", new System.Net.Http.HttpRequestException(get.StatusCode.ToString()));
                        else
                            return await get.Content.ReadAsStringAsync();
                    }
                }
                catch(System.Runtime.InteropServices.COMException ce)
                {
                    throw new ConnectException($"在{nameof(GetAsync)}中出现错误", ce);
                }
                catch (Exception e)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new ConnectException("待处理异常", e);
                }
            });
        }
        public static IAsyncOperation<Stream> GetAsyncAsStream(Uri uri)
        {
            return Run(async token =>
            {
                var postTask = Client.GetAsync(uri);
                token.Register(() => postTask.AsAsyncOperation().Cancel());
                try
                {
                    using (var get = await postTask)
                    {
                        if (!get.IsSuccessStatusCode)
                            throw new ConnectException("在HttpRequest中出现错误", new System.Net.Http.HttpRequestException(get.StatusCode.ToString()));
                        else
                            return await get.Content.ReadAsStreamAsync();
                    }
                }
                catch (System.Runtime.InteropServices.COMException ce)
                {
                    throw new ConnectException($"在{nameof(GetAsync)}中出现错误", ce);
                }
                catch (Exception e)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new ConnectException("待处理异常", e);
                }
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

        #region Extended Methods
        public static void ParseAdd(this HttpContentHeaders headers, string key, string value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
