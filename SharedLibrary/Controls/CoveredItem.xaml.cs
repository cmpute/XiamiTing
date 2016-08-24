using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace JacobC.Xiami.Controls
{
    public sealed partial class CoveredItem : UserControl
    {
        public CoveredItem()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 获取或设置封面展示的数据来源
        /// </summary>
        public ICovered ItemSource
        {
            get { return GetValue(ItemSourceProperty) as ICovered; }
            set { SetValue(ItemSourceProperty, value); }
        }
        private static readonly ICovered _defaultItemSource = default(ICovered);
        /// <summary>
        /// 标识<see cref="ItemSource"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty ItemSourceProperty =
              DependencyProperty.Register(nameof(ItemSource), typeof(ICovered),
                  typeof(CoveredItem), new PropertyMetadata(_defaultItemSource, (d, e) =>
                  {
                      (d as CoveredItem).InternalItemSourceChanged(e);
                  }));
        private void InternalItemSourceChanged(DependencyPropertyChangedEventArgs e) { }

    }
}
