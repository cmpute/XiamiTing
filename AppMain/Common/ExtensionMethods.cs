using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace JacobC
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 将<see cref="DependencyPropertyChangedEventArgs"/>转化成<see cref="ChangedEventArgs{TValue}"/>类型
        /// </summary>
        public static ChangedEventArgs<T> ToChangedEventArgs<T>(this DependencyPropertyChangedEventArgs e) => new ChangedEventArgs<T>((T)e.OldValue, (T)e.NewValue);
        /// <summary>
        /// 将<see cref="Color"/>类转化为对应颜色的<see cref="SolidColorBrush"/>
        /// </summary>
        public static SolidColorBrush ToSolidColorBrush(this Color color) => new SolidColorBrush(color);

        public static List<DependencyObject> AllChildren(this DependencyObject parent)
        {
            var list = new List<DependencyObject>();
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                list.Add(child);
                list.AddRange(AllChildren(child));
            }
            return list;
        }
    }
}
