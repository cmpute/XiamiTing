using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace JacobC
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 将<see cref="DependencyPropertyChangedEventArgs"/>转化成<see cref="ChangedEventArgs{TValue}"/>类型
        /// </summary>
        public static ChangedEventArgs<T> ToChangedEventArgs<T>(this DependencyPropertyChangedEventArgs e) => new ChangedEventArgs<T>((T)e.OldValue, (T)e.NewValue);
    }
}
