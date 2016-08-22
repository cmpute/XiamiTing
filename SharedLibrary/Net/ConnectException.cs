using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Net
{
    /// <summary>
    /// 表示在爬取信息中出现的错误
    /// </summary>
    public class ConnectException : Exception
    {
        public ConnectException() : base() { }
        public ConnectException(string message) : base(message) { }
        public ConnectException(string message, Exception innerException) : base(message, innerException) { }
    }
}
