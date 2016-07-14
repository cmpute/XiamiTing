using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Template10.Common;
using Template10.Services.SettingsService;
using Windows.UI.Xaml;

namespace JacobC.Xiami
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 读取设置后删除该设置
        /// </summary>
        public static T ReadAndReset<T>(this SettingsHelper setting,string key, T otherwise = default(T), SettingsStrategies strategy = SettingsStrategies.Local)
        {
            T val = setting.Read<T>(key,otherwise, strategy);
            setting.Remove(key, strategy);
            return val;
        }
        /// <summary>
        /// 将对象转换成枚举类型
        /// </summary>
        public static T ParseEnum<T>(object value) => (T)(Enum.Parse(typeof(T), value.ToString()));
        /// <summary>
        /// 向控制台输出窗口输出记录
        /// </summary>
        /// <param name="text">记录内容</param>
        /// <param name="preffix">消息类型(Error, Info等)</param>
        /// <param name="target">操作目标</param>
        /// <param name="caller">调用成员名</param>
        public static void ConsoleLog(string text = "", string preffix = "Log", string target = null, [CallerMemberName]string caller = null)
        {
            string o = $"[{DateTime.Now.ToString("T")} {preffix} caller:{caller}]: {text}";
            if (target != null) o += $"[target:{target}]";
            Debug.WriteLine(o);
        }
        /// <summary>
        /// 将<see cref="DependencyPropertyChangedEventArgs"/>类型转换成<see cref="ChangedEventArgs{TValue}"类型/>
        /// </summary>
        /// <typeparam name="T">ChangedEventArgs参数类型</typeparam>
        public static ChangedEventArgs<T> ToChangedEventArgs<T>(this DependencyPropertyChangedEventArgs e)
            => new ChangedEventArgs<T>((T)e.OldValue, (T)e.NewValue);
    }
}
