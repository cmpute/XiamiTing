using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 表示登录结果信息的对象
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// 创建登录信息的对象
        /// </summary>
        /// <param name="status">登录结果</param>
        /// <param name="taobaousername">（如果需要淘宝登录的话）淘宝用户名</param>
        public LoginResult(LoginStatus status, string taobaousername = null)
        {
            Status = status;
            TaoBaoUserName = taobaousername;
        }
        public LoginStatus Status { get; private set; }
        public string TaoBaoUserName { get; private set; }
    }

    /// <summary>
    /// 标识登录的结果
    /// </summary>
    public enum LoginStatus
    {
        Success,
        NeedTaobaoLogin,
        /// <summary>
        /// 登录失败，一般是密码错误或证书错误
        /// </summary>
        Failed,
        LoggedInAlready,
        Unknown
    }
}
