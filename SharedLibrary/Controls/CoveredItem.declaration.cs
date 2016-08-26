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
using JacobC.Xiami.Services;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Documents;
using System.Collections.ObjectModel;

namespace JacobC.Xiami.Controls
{
    public sealed partial class CoveredItem : UserControl
    {
        #region Public Properties

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
        private void InternalImageSizeChanged(ChangedEventArgs<double> e) { TotalHeight = ImageSize + InfoPanelHeight; }

        /// <summary>
        /// 获取或设置信息栏高度属性
        /// </summary>
        public double InfoPanelHeight
        {
            get { return (double)GetValue(InfoPanelHeightProperty); }
            set { SetValue(InfoPanelHeightProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="InfoPanelHeight"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty InfoPanelHeightProperty =
              DependencyProperty.Register(nameof(InfoPanelHeight), typeof(double),
                  typeof(CoveredItem), new PropertyMetadata(24d, (d, e) =>
                  {
                      //(d as CoveredItem).InfoPanelHeightChanged?.Invoke(d, e.ToChangedEventArgs<double>());
                      (d as CoveredItem).InternalInfoPanelHeightChanged(e);
                  }));
        private void InternalInfoPanelHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            TotalHeight = ImageSize + InfoPanelHeight;
            if (InfoPanelHeight == 0)
                TextGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 获取或设置信息栏内容属性
        /// </summary>
        public InlineCollection InfoPanelContent
        {
            get { return (InlineCollection)GetValue(InfoPanelContentProperty); }
            set { SetValue(InfoPanelContentProperty, value); }
        }
        public static readonly DependencyProperty InfoPanelContentProperty =
            DependencyProperty.Register("InfoPanelContent", typeof(InlineCollection), typeof(CoveredItem), new PropertyMetadata(null));

        /// <summary>
        /// 按下专辑显示部分时发生, sender为ItemSource
        /// </summary>
        public event EventHandler<RoutedEventArgs> MainPanelClick;


        public object InfoContent
        {
            get { return (object)GetValue(InfoContentProperty); }
            set { SetValue(InfoContentProperty, value); }
        }
        public static readonly DependencyProperty InfoContentProperty =
            DependencyProperty.Register("InfoContent", typeof(object), typeof(CoveredItem), new PropertyMetadata(null));

        public DataTemplate InfoContentTemplate
        {
            get { return (DataTemplate)GetValue(InfoContentTemplateProperty); }
            set { SetValue(InfoContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty InfoContentTemplateProperty =
            DependencyProperty.Register("InfoContentTemplate", typeof(DataTemplate), typeof(CoveredItem), new PropertyMetadata(null));

        public DataTemplateSelector InfoContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(InfoContentTemplateSelectorProperty); }
            set { SetValue(InfoContentTemplateSelectorProperty, value); }
        }
        public static readonly DependencyProperty InfoContentTemplateSelectorProperty =
            DependencyProperty.Register("InfoContentTemplateSelector", typeof(DataTemplateSelector), typeof(CoveredItem), new PropertyMetadata(null));

        #endregion
    }

}
