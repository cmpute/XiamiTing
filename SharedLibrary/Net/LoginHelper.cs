using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Newtonsoft.Json;
using HtmlAgilityPack;

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
            //pcontent.Headers.Add("Referer", "https://login.xiami.com/member/login");
            pcontent.Headers.Add("Origin", "https://login.xiami.com/");
            var callback = await HttpHelper.PostAsync("https://login.xiami.com/member/login?callback=jQuery", pcontent);
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
            var response = await HttpHelper.GetAsync("https://passport.alipay.com/mini_login.htm?lang=&appName=xiami&appEntrance=taobao&cssLink=&styleType=vertical&bizParams=&notLoadSsoView=&notKeepLogin=&rnd=0.6477347570091512?lang=zh_cn&appName=xiami&appEntrance=taobao&cssLink=https%3A%2F%2Fh.alipayobjects.com%2Fstatic%2Fapplogin%2Fassets%2Flogin%2Fmini-login-form-min.css%3Fv%3D20140402&styleType=vertical&bizParams=&notLoadSsoView=true&notKeepLogin=true&rnd=0.9090916193090379");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            var form = doc.DocumentNode.SelectSingleNode("/html/body/div");
            Func<string, string> GetValue = (string name) => form.SelectSingleNode($".//input[@name='{name}']").GetAttributeValue("value", "");
            var data = new Dictionary<string, string>()
            {
                ["loginId"] = username,
                ["password"] = password,
                ["appname"] = "xiami",
                ["appEntrance"] = "taobao",
                ["hsid"] = GetValue("hsid"),
                ["cid"] = GetValue("cid"),
                ["rdsToken"] = GetValue("rdsToken"),
                ["umidToken"] = GetValue("umidToken"),
                ["_csrf_token"] = GetValue("_csrf_token"),
                ["checkCode"] = ""
            };
            response = await HttpHelper.PostAsync("https://passport.alipay.com/newlogin/login.do?fromSite=0",
                new FormUrlEncodedContent(data),
                (headers)=> headers.Referrer=new Uri("https://passport.alipay.com/mini_login.htm"));
            System.Diagnostics.Debugger.Break();
            
            return new LoginResult(LoginStatus.Unknown);
        }
        public static async Task Logout()
        {
            if(IsLoggedIn)
                await HttpHelper.GetAsync("http://www.xiami.com/member/logout");
        }
        public static async Task<LoginResult> CheckNeedTaobaoLogin(string useremail)
        {
            var callback = await HttpHelper.GetAsync($"https://login.xiami.com/accounts/checkxiaminame?email={WebUtility.HtmlEncode(useremail)}");
            _logincallback callbackdata = JsonConvert.DeserializeObject<_logincallback>(callback);
            if (callbackdata.status)
                return new LoginResult(LoginStatus.Success);
            else
                return new LoginResult(LoginStatus.NeedTaobaoLogin, callbackdata.data.taobao_nick);
            
        }
        public static async Task<LoginResult> CheckTaobaoUsernameExist(string username)
        {
            //https://passport.alipay.com/newlogin/account/check.do?fromSite=-2
            throw new NotImplementedException();
        }
        public static async Task<string> GetToken()
        {
            var cookies = HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain);
            if (cookies["_xiamitoken"] == null)
                await HttpHelper.GetAsync(HttpHelper.XiamiDomain.ToString());
            return HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain)["_xiamitoken"].Value;
        }

        //TODO:如果保存了Cookie的话自动读取
        public static string NickName { get; private set; }
        public static uint UserId { get; private set; }
        public static bool IsLoggedIn { get; private set; }
    }
}
