using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 提供一些参数的转化
    /// </summary>
    public static class ParamHelper
    {
        public static double GetTimestamp(DateTime now) => (now - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        public static string GetTimestamp() => ((int)GetTimestamp(DateTime.Now)).ToString();
        //为WindowsApi提供，似乎失效了
        public static string GetApiSignature(Dictionary<string, string> dic, string secret)
        {
            var res = "";
            var keys = dic.Keys.OrderBy(x => x);
            foreach (var k in keys)
            {
                res += k + dic[k];
            }
            res += secret;
            res = Encoding.UTF8.GetBytes(res).ToMD5();
            //res = Encoding.Default.GetBytes(res).ToMD5();
            return res;
        }
        public static string ToMD5(this byte[] input)
        {
            var md5Hasher = System.Security.Cryptography.MD5.Create();
            byte[] data = md5Hasher.ComputeHash(input);
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }
        public static string ToMD5(this string input)
        {
            var md5Hasher = System.Security.Cryptography.MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("gbk").GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }
    }
}
