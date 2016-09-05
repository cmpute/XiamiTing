using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JacobC.Xiami.Services
{
    public static class LogService
    {
        /// <summary>
        /// 向调试输出台写入一行
        /// </summary>
        /// <param name="text">记录内容</param>
        /// <param name="sourcetype">记录来源的类别，如Template10, MediaPlayer等等</param>
        /// <param name="caller">调用成员名</param>
        public static void DebugWrite(string text, string sourcetype="Log", [CallerMemberName]string caller = null)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now.ToString("T")} {sourcetype}][caller:{caller}] {text}");
#endif
        }

        /// <summary>
        /// 向调试输出台写入错误
        /// </summary>
        /// <returns>是否处理了错误</returns>
        public static bool ErrorWrite(Exception e, string sourcetype = "Log", [CallerMemberName]string caller = null)
        {
#if DEBUG
            DebugWrite($"{e.Message} [inner:{e.InnerException?.Message}]", $"{sourcetype} Error", caller);
            return false;
#else
            return true;
#endif
        }
    }
}
