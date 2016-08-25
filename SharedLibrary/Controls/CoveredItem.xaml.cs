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

namespace JacobC.Xiami.Controls
{
    /// <summary>
    /// 在列表中展示有封面内容的控件
    /// </summary>
    /// <remarks>
    /// TODO: 增加属性决定上半部分内容的出现方式？
    /// TODO: 弄清高度动画
    /// TODO: 增加按键事件，和按键可见性
    /// </remarks>
    public sealed partial class CoveredItem : UserControl
    {
        public CoveredItem()
        {
            this.InitializeComponent();
        }
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
        /// 获取或设置InfoPanelHeight属性
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
        private void InternalInfoPanelHeightChanged(DependencyPropertyChangedEventArgs e) { TotalHeight = ImageSize + InfoPanelHeight; }

        private double TotalHeight
        {
            get { return (double)GetValue(TotalHeightProperty); }
            set { SetValue(TotalHeightProperty, value); }
        }
        public static readonly DependencyProperty TotalHeightProperty =
            DependencyProperty.Register("TotalHeight", typeof(double), typeof(CoveredItem), new PropertyMetadata(124d));

        #endregion
        bool _hoverdown = false, _hoverup = false;
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            if (_hoverdown)
                return;
            _hoverup = true;
            LogService.DebugWrite("PinterEnteredUp");
            VisualStateManager.GoToState(this, "PointerOverUp", true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            _hoverup = false;
            VisualStateManager.GoToState(this, "NormalUp", true);
        }

        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            LogService.DebugWrite("PinterEnteredDown");
            _hoverdown = true;
            VisualStateManager.GoToState(this, "PointerOverDown", true);
            if (_hoverup)
                OnPointerExited(e);
        }

        private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _hoverdown = false;
            VisualStateManager.GoToState(this, "NormalDown", true);
        }
    }

    internal class GridLengthCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new GridLength((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    internal class MinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return -(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
