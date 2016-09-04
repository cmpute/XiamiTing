using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace JacobC.Xiami.Net
{
    public static class LoginHelper
    {
        public static async Task XiamiLogin(string username, string password)
        {
            string content = "";
            await HttpHelper.PostAsync(new Uri("https://login.xiami.com/member/login?callback=jQuery"), content);
        }

        public static async Task<string> GetToken()
        {
            var cookies = HttpHelper.Handler.CookieContainer.GetCookies(HttpHelper.XiamiDomain);
            if (cookies["_xiamitoken"] == null)
                await HttpHelper.GetAsync(HttpHelper.XiamiDomain);
            return cookies["_xiamitoken"].Value;
        }
    }
}
