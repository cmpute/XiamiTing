using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Newtonsoft.Json;

namespace JacobC.Xiami.Net
{
    public static class LoginHelper
    {
        [JsonObject(MemberSerialization = MemberSerialization.Fields)]
        class _logincallback
        {
            public bool status;
            public string message;
            [JsonObject(MemberSerialization = MemberSerialization.Fields)]
            public class _logindata
            {
                public string user_id, nick_name, taobao_nick;
            }
            public _logindata data;
        }

        public static async Task<LoginResult> XiamiLogin(string useremail, string password)
        {
            var token = await GetToken();
            string content = $"_xiamitoken={token}&done=http%253A%252F%252Fwww.xiami.com&from=web&havanaId=&email={WebUtility.UrlEncode(useremail)}&password={WebUtility.UrlEncode(password)}&submit=%E7%99%BB+%E5%BD%95";
            HttpContent pcontent = new StringContent(content);
            pcontent.Headers.Add("Referrer", "https://login.xiami.com/member/login");
            pcontent.Headers.Add("Origin", "https://login.xiami.com/");
            pcontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var callback = await HttpHelper.PostAsync(new Uri("https://login.xiami.com/member/login?callback=jQuery"), pcontent);
            _logincallback callbackdata = JsonConvert.DeserializeObject<_logincallback>(callback.Substring(8, callback.Length - 9));//"JQuery(...)"
            var success = callbackdata.status;
            if (success)
            {
                NickName = WebUtility.HtmlDecode(callbackdata.data.nick_name);
                UserId = uint.Parse(callbackdata.data.user_id);
                IsLoggedIn = true;
                return new LoginResult(LoginStatus.Success);
            }
            else if (callbackdata.message.StartsWith("请使用淘宝"))
                return new LoginResult(LoginStatus.NeedTaobaoLogin,
                    callbackdata.message.Substring(7, callbackdata.message.Length - 10));
            else
                return new LoginResult(LoginStatus.Unknown);
        }
        public static async Task<LoginResult> TaoBaoLogin(string username,string password)
        {
            throw new NotImplementedException();
        }
        public static async Task Logout()
        {
            if(IsLoggedIn)
                await HttpHelper.GetAsync(new Uri("http://www.xiami.com/member/logout"));
        }
        public static async Task<LoginResult> CheckNeedTaobaoLogin(string useremail)
        {
            var callback = await HttpHelper.GetAsync(new Uri($"https://login.xiami.com/accounts/checkxiaminame?email={WebUtility.HtmlEncode(useremail)}"));
            _logincallback callbackdata = JsonConvert.DeserializeObject<_logincallback>(callback);
            if (callbackdata.status)
                return new LoginResult(LoginStatus.Success);
            else
                return new LoginResult(LoginStatus.NeedTaobaoLogin, callbackdata.data.taobao_nick);
            
        }
        public static async Task<string> GetToken()
        {
            var cookies = HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain);
            if (cookies["_xiamitoken"] == null)
                await HttpHelper.GetAsync(HttpHelper.XiamiDomain);
            return HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain)["_xiamitoken"].Value;
        }

        //TODO:如果保存了Cookie的话自动读取
        public static string NickName { get; private set; }
        public static uint UserId { get; private set; }
        public static bool IsLoggedIn { get; private set; }
    }
}
