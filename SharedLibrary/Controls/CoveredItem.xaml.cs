using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
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

        /// <summary>
        /// 获取或设置ImageSize属性
        /// </summary>
        public double ImageSize
        {
            get { return (double)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }
        private static readonly double _defaultImageSize = 100;
        /// <summary>
        /// 标识<see cref="ImageSize"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty ImageSizeProperty =
              DependencyProperty.Register(nameof(ImageSize), typeof(double),
                  typeof(CoveredItem), new PropertyMetadata(_defaultImageSize, (d, e) =>
                  {
                      (d as CoveredItem).ImageSizeChanged?.Invoke(d, e.ToChangedEventArgs<double>());
                      (d as CoveredItem).InternalImageSizeChanged(e.ToChangedEventArgs<double>());
                  }));
        /// <summary>
        /// 在<see cref="ImageSize"/>属性发生变更时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<double>> ImageSizeChanged;
        partial void InternalImageSizeChanged(ChangedEventArgs<double> e);

    }
}
