using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Services.SettingsService;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Data.Xml.Dom;
using System.Collections.Generic;
using System.Text;
using System.Net;
using HtmlAgilityPack;

namespace JacobC.Xiami
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 读取设置后删除该设置
        /// </summary>
        public static T ReadAndReset<T>(this ISettingsService setting, string key, T otherwise = default(T))
        {
            T val = setting.Read<T>(key, otherwise);
            setting.Remove(key);
            return val;
        }

        /// <summary>
        /// 将对象转换成枚举类型
        /// </summary>
        public static T ParseEnum<T>(object value) => (T)(Enum.Parse(typeof(T), value.ToString()));

        /// <summary>
        /// 将<see cref="DependencyPropertyChangedEventArgs"/>类型转换成<see cref="ChangedEventArgs{TValue}"类型/>
        /// </summary>
        /// <typeparam name="T">ChangedEventArgs参数类型</typeparam>
        public static ChangedEventArgs<T> ToChangedEventArgs<T>(this DependencyPropertyChangedEventArgs e)
            => new ChangedEventArgs<T>((T)e.OldValue, (T)e.NewValue);

        /// <summary>
        /// 使async方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的async方法</param>
        public static void InvokeAndWait(Func<Task> asyncMethod)
        {
            Task.Run(() => asyncMethod())
                .ContinueWith(task => task.Wait())
                .Wait();
        }
        /// <summary>
        /// 使返回<see cref="IAsyncAction"/>的无参方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的方法</param>
        public static void InvokeAndWait(Func<IAsyncAction> asyncMethod) => InvokeAndWait(async () => await asyncMethod());
        /// <summary>
        /// 使async方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的async方法</param>
        public static T InvokeAndWait<T>(Func<Task<T>> asyncMethod)
        {
            Task<T> t = Task.Run(() => asyncMethod())
                .ContinueWith(task =>
                {
                    task.Wait();
                    return task.Result;
                });
            t.Wait();
            return t.Result;
        }
        /// <summary>
        /// 使返回<see cref="IAsyncOperation{TResult}"/>的无参方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的方法</param>
        public static T InvokeAndWait<T>(Func<IAsyncOperation<T>> asyncMethod) => InvokeAndWait(async () => await asyncMethod());

        /// <summary>
        /// 获取指定类型的参数
        /// </summary>
        /// <typeparam name="T">需要获取的参数类型(int不能用！)</typeparam>
        public static T GetParameter<T>(this NavigationEventArgs e)
        {
            return Template10.Services.SerializationService.SerializationService.Json.Deserialize<T>(e.Parameter?.ToString());
        }

        /// <summary>
        /// 获取指定名称的子节点
        /// </summary>
        /// <param name="node">父节点</param>
        /// <param name="name">子节点名称</param>
        /// <returns>第一个匹配指定名称的子节点</returns>
        public static IXmlNode Element(this IXmlNode node, string name)
        {
            foreach (var item in node.ChildNodes)
            {
                if (item.LocalName?.ToString() == name)
                    return item;
            }
            return null;
        }
        /// <summary>
        /// 获取指定名称子节点的InnerText
        /// </summary>
        /// <param name="node">父节点</param>
        /// <param name="name">子节点名称</param>
        /// <returns></returns>
        public static string ElementText(this IXmlNode node, string name) => Element(node, name).InnerText;
        /// <summary>
        /// 获取指定名称所有的子节点
        /// </summary>
        /// <param name="node">父节点</param>
        /// <param name="name">子节点名称</param>
        /// <returns></returns>
        public static IEnumerable<IXmlNode> Elements(this IXmlNode node, string name)
        {
            foreach (var item in node.ChildNodes)
                if (item.LocalName?.ToString() == name)
                    yield return item;
        }
        public static HtmlNode Descendant(this HtmlNode mnode, string name) => InnerDescendant(mnode, name.ToLowerInvariant());
        private static HtmlNode InnerDescendant(HtmlNode mnode, string name)
        {
            foreach (HtmlNode node in mnode.ChildNodes)
                if (node.Name.Equals(name))
                    return node;
                else
                {
                    var res = InnerDescendant(node, name);
                    if (res != null) return res;
                }
            return null;
        }

        /// <summary>
        /// 将字典对象转化成Html的Query格式字符串
        /// </summary>
        public static string ToQueryString(this Dictionary<string, string> dic, bool encode = true)
        {
            bool isfirst = true;
            StringBuilder res = new StringBuilder();
            foreach (var item in dic)
            {
                if (isfirst)
                    isfirst = false;
                else
                    res.Append("&");
                res.Append(encode ? WebUtility.UrlEncode(item.Key) : item.Key);
                res.Append("=");
                res.Append(encode ? WebUtility.UrlEncode(item.Value) : item.Value);
            }
            return res.ToString();
        }

        public static IEnumerable<T> ToEnumerable<T>(this T target)
        {
            yield return target;
        }
    }
}
