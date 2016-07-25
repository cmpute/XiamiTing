using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation;
using Windows.Web.Http;

namespace JacobC.Xiami.Net
{
    public static class HttpHelper
    {
        public static IAsyncOperation<string> PostAsync(HttpClient httpClient, Uri uri, string request)
        {
            if (httpClient == null)
                httpClient = new HttpClient();
            return Run(async token =>
            {
                using (var re = new HttpStringContent(request))
                {
                    re.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                    var postTask = httpClient.PostAsync(uri, re);
                    token.Register(() => postTask.Cancel());
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
                }
            });
        }
        public static IAsyncOperation<string> PostAsync(Uri uri, string request) => PostAsync(null, uri, request);

        public static IAsyncOperation<string> GetAsync(this HttpClient httpClient, Uri uri)
        {
            if (httpClient == null)
                httpClient = new HttpClient();
            return Run(async token =>
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                var postTask = httpClient.GetAsync(uri);
                token.Register(() => postTask.Cancel());
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
        public static IAsyncOperation<string> GetAsync(Uri uri) => GetAsync(null, uri);
    }
}
