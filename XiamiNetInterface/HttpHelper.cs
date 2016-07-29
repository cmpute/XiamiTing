using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation;
using System.Net.Http;
using JacobC.Xiami.Services;

namespace JacobC.Xiami.Net
{
    public static class HttpHelper
    {
        //TODO: 采用gzip
        static HttpClientHandler _handler;
        /// <summary>
        /// 获取控制Http请求属性的<see cref=""/>
        /// </summary>
        /// <remarks>设置后可能需要重启应用生效</remarks>
        public static HttpClientHandler Handler
        {
            get
            {
                if(_handler == null)
                    _handler = new HttpClientHandler();
                
                return _handler;
            }
        }

        public static void ClearCookies()
        {
            _handler = new HttpClientHandler();
            _client = new HttpClient(_handler);
        }
        public static void PrintCookies() => PrintCookies(new Uri("http://www.xiami.com"));
        public static void PrintCookies(Uri domain)
        {
            var container = Handler.CookieContainer;
            foreach (System.Net.Cookie item in container.GetCookies(domain))
            {
                LogService.DebugWrite(item.ToString(), "Cookie");
            }
        }

        static HttpClient _client;
        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient(Handler);
                    _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                }
                return _client;
            }
        }

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
                            throw new ConnectException("在HttpRequest中出现错误", new System.Net.Http.HttpRequestException(get.StatusCode.ToString()));
                        else
                            return await get.Content.ReadAsStringAsync();
                    }
                }
                catch (System.Runtime.InteropServices.COMException ce)
                {
                    throw new ConnectException($"在{nameof(GetAsync)}中出现错误", ce);
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
                var postTask = Client.GetAsync(uri);
                token.Register(() => postTask.AsAsyncOperation().Cancel());
                try
                {
                    using (var get = await postTask)
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
            });
        }
    }
}
