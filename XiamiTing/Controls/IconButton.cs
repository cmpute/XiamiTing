using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace JacobC.Xiami.Controls
{
    public class IconButton : Button
    {
        public IconButton()
        {
            DefaultStyleKey = typeof(IconButton);
        }

        /// <summary>
        /// 获取或设置PointerOver时的前景色
        /// </summary>
        public Brush HoverForeground
        {
            get { return GetValue(HoverForegroundProperty) as Brush; }
            set { SetValue(HoverForegroundProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="HoverForeground"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty HoverForegroundProperty =
              DependencyProperty.Register(nameof(HoverForeground), typeof(Brush),
                  typeof(IconButton), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        ///// <summary>
        ///// 获取或设置Icon属性
        ///// </summary>
        //public IconElement Icon
        //{
        //    get { return GetValue(IconProperty) as IconElement; }
        //    set { SetValue(IconProperty, value); }
        //}
        //private static readonly IconElement _defaultIcon = default(IconElement);
        ///// <summary>
        ///// 标识<see cref="Icon"/>依赖属性
        ///// </summary>
        //public static readonly DependencyProperty IconProperty =
        //      DependencyProperty.Register(nameof(Icon), typeof(IconElement),
        //          typeof(IconButton), new PropertyMetadata(default(IconElement)));

    }
}
