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
using Windows.UI.Xaml.Markup;
using System.Windows.Input;

namespace JacobC.Xiami.Controls
{
    /// <summary>
    /// 在列表中展示有封面内容的控件
    /// </summary>
    /// <remarks>
    /// TODO: 增加属性决定上半部分内容的出现方式？
    /// TODO: 增加按键事件，和按键可见性
    /// </remarks>
    public sealed partial class CoveredItem : UserControl
    {
        public CoveredItem()
        {
            this.InitializeComponent();
        }

        private double TotalHeight
        {
            get { return (double)GetValue(TotalHeightProperty); }
            set { SetValue(TotalHeightProperty, value);}
        }
        public static readonly DependencyProperty TotalHeightProperty =
            DependencyProperty.Register("TotalHeight", typeof(double), typeof(CoveredItem), new PropertyMetadata(124d));


        bool _hoverdown = false, _hoverup = false;
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            if (_hoverdown)
                return;
            _hoverup = true;
            //LogService.DebugWrite("PointerEnteredUp", nameof(CoveredItem));
            VisualStateManager.GoToState(this, "PointerOverUp", true);
        }
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            _hoverup = false;
            VisualStateManager.GoToState(this, "NormalUp", true);
        }

        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //LogService.DebugWrite("PointerEnteredDown", nameof(CoveredItem));
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

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlaybackService.Instance.PlayModel(this.ItemSource);
        }
        private void CommandGrid_Click(object sender, RoutedEventArgs e)
        {
            LogService.DebugWrite("Clicked", nameof(CoveredItem));
            MainPanelClick?.Invoke(ItemSource, e);
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistService.Instance.AddModel(this.ItemSource);
        }
    }

}
