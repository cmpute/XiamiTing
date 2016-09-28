using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using System.Security.Cryptography;
using Newtonsoft.Json;
using HtmlAgilityPack;
using Template10.Common;
using JacobC.Xiami.Services;

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

        static LoginHelper()
        {
            ReadAccountInfo();
        }

        public static async Task<LoginResult> XiamiLogin(string useremail, string password)
        {
            if (CheckMemberAuth())
                return new LoginResult(LoginStatus.LoggedInAlready);
            var token = await GetToken();
            string content = $"_xiamitoken={token}&done=http%253A%252F%252Fwww.xiami.com&from=web&havanaId=&email={WebUtility.UrlEncode(useremail)}&password={WebUtility.UrlEncode(password)}&submit=%E7%99%BB+%E5%BD%95";
            HttpContent pcontent = new StringContent(content);
            //pcontent.Headers.Add("Referer", "https://login.xiami.com/member/login");
            pcontent.Headers.Add("Origin", "https://login.xiami.com/");
            var callback = await HttpHelper.PostAsync("https://login.xiami.com/member/login?callback=jQuery", pcontent, (headers) => headers.Referrer = new Uri("https://login.xiami.com/member/login"));
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
        public static async Task<LoginResult> XiamiLogin(string taobaoCallback_st)
        {
            if (CheckMemberAuth())
                return new LoginResult(LoginStatus.LoggedInAlready);
            var response = await HttpHelper.SendMessageInternal($"http://www.xiami.com/accounts/back?st={taobaoCallback_st}&done=http%3A%2F%2Fwww.xiami.com%2F%2F", HttpMethod.Get, null, null);
            await response.Content.ReadAsStringAsync();
            IsLoggedIn = CheckMemberAuth();
            return new LoginResult(IsLoggedIn ? LoginStatus.Success : LoginStatus.Failed);
        }
        public static async Task Logout()
        {
            if(IsLoggedIn)
                await HttpHelper.GetAsync("http://www.xiami.com/member/logout");
            IsLoggedIn = CheckMemberAuth();
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
        public static async Task<string> GetToken()
        {
            var cookies = HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain);
            if (cookies["_xiamitoken"] == null)
                await HttpHelper.GetAsync(HttpHelper.XiamiDomain.ToString());
            return HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain)["_xiamitoken"].Value;
        }
        private static bool CheckMemberAuth()
        {
            return !(HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain)["member_auth"] == null);
        }

        #region Taobao Login Part [弃置改用WebView]
        /*
        public static byte[] ConvertFrom16String(string base16)
        {
            List<byte> res = new List<byte>();
            int p = base16.Length - 2;
            for (; p >= 0; p -= 2)
                res.Insert(0, Convert.ToByte(base16.Substring(p, 2), 16));
            if (p == -1)
                res.Insert(0, Convert.ToByte("0" + base16[0], 16));
            return res.ToArray();
        }
        public static string ConvertTo16String(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();
            foreach (var item in bytes)
            {
                var str = Convert.ToString(item, 16);
                switch (str.Length)
                {
                    case 1:
                        s.Append("0");
                        s.Append(str);
                        break;
                    case 0:
                        s.Append("00");
                        break;
                    default:
                        s.Append(str);
                        break;
                }
            }
            return s.ToString();
        }
        public static async Task<LoginResult> CheckTaobaoUsernameExist(string username)
        {
            //https://passport.alipay.com/newlogin/account/check.do?fromSite=-2
            throw new NotImplementedException();
        }
        public static async Task<LoginResult> TaoBaoLogin(string username, string password)
        {
            var response = await HttpHelper.GetAsync("https://passport.alipay.com/mini_login.htm?lang=&appName=xiami&appEntrance=taobao&cssLink=&styleType=vertical&bizParams=&notLoadSsoView=&notKeepLogin=&rnd=0.6477347570091512?lang=zh_cn&appName=xiami&appEntrance=taobao&cssLink=https%3A%2F%2Fh.alipayobjects.com%2Fstatic%2Fapplogin%2Fassets%2Flogin%2Fmini-login-form-min.css%3Fv%3D20140402&styleType=vertical&bizParams=&notLoadSsoView=true&notKeepLogin=true&rnd=" + new Random().NextDouble().ToString());
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            var form = doc.DocumentNode.SelectSingleNode("/html/body/div");
            Func<string, string> GetValue = (string name) => form.SelectSingleNode($".//input[@name='{name}']").GetAttributeValue("value", "");
            string encrypt = "";
            try
            {
                using (var rsa = RSA.Create())
                {
                    Encoding provider = Encoding.UTF8;
                    RSAParameters publickey = new RSAParameters()
                    {
                        Modulus = ConvertFrom16String(GetValue("modulus")),
                        Exponent = Convert.FromBase64String("AQAB")
                    };
                    rsa.ImportParameters(publickey);
                    encrypt = ConvertTo16String(rsa.Encrypt(provider.GetBytes(password), RSAEncryptionPadding.Pkcs1)); ;
                    System.Diagnostics.Debugger.Break();
                }
            }
            catch
            { System.Diagnostics.Debugger.Break(); }
            var data = new Dictionary<string, string>()
            {
                ["loginId"] = username,
                //["password"] = password,
                ["password2"] = encrypt,
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
                (headers) => headers.Referrer = new Uri("https://passport.alipay.com/mini_login.htm"));
            System.Diagnostics.Debugger.Break();



            return new LoginResult(LoginStatus.Unknown);
        }
        */
        #endregion

        //TODO:如果保存了Cookie的话自动读取
        public static string NickName { get; private set; }
        static uint userid = 0;
        public static uint UserId
        {
            get { return userid; }
            set
            {
                if (userid != value)
                {
                    var e = new ChangedEventArgs<uint>(userid, value);
                    userid = value;
                    UserChanged?.Invoke(null, e);
                }
            }
        }
        /// <summary>
        /// 用户更改后发生
        /// </summary>
        public static event EventHandler<ChangedEventArgs<uint>> UserChanged;
        /// <summary>
        /// 用户登录状态发生改变时发生
        /// </summary>
        public static event EventHandler<ChangedEventArgs<bool>> LogStateChanged;
        static bool _logged = false;
        public static bool IsLoggedIn
        {
            get { return _logged; }
            private set
            {
                if (_logged != value)
                {
                    var e = new ChangedEventArgs<bool>(_logged, value);
                    _logged = value;
                    LogStateChanged?.Invoke(null, e);
                }
            }
        }

        public static void SaveAccountInfo()
        {
            if (NickName != default(string)) SettingsService.Account.Write(nameof(NickName), NickName);
            if (UserId != 0) SettingsService.Account.Write(nameof(UserId), UserId);
            if (IsLoggedIn != false) SettingsService.Account.Write(nameof(IsLoggedIn), IsLoggedIn);
        }
        public static void ReadAccountInfo(bool reset = true)
        {
            if(reset)
            {
                NickName = SettingsService.Account.ReadAndReset<string>(nameof(NickName));
                UserId = SettingsService.Account.ReadAndReset(nameof(UserId), 0u);
                IsLoggedIn = SettingsService.Account.ReadAndReset(nameof(IsLoggedIn), false);
            }
        }

    }
}
