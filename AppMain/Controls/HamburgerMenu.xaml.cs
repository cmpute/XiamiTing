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
using JacobC;

namespace JacobC.Controls
{
    /// <summary>
    /// 汉堡菜单控件
    /// </summary>
    /// <remarks>
    /// 参考了Template10库的汉堡菜单
    /// </remarks>
    public sealed partial class HamburgerMenu : UserControl
    {
        public HamburgerMenu()
        {
            this.InitializeComponent();
        }

        #region Private Members

        private static void WriteDebug(string v, DependencyPropertyChangedEventArgs e)
        {
#if Debug
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay.ToString()} OldValue: {e.OldValue} NewValue: {e.NewValue}");
#endif
        }

        #endregion

        #region Projection Properties

        /// <summary>
        /// 该属性为内部SpiltView的DisplayMode的投影属性
        /// </summary>
        /// <remarks>
        /// 该属性一般由VisualState自动设置，但也可以在定制控件时手动设置
        /// </remarks>
        public SplitViewDisplayMode DisplayMode
        {
            get { return (SplitViewDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(nameof(DisplayMode), typeof(SplitViewDisplayMode),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
                {
                    WriteDebug(nameof(DisplayMode), e);
                    (d as HamburgerMenu).DisplayModeChanged?.Invoke(d, e.ToChangedEventArgs<SplitViewDisplayMode>());
                    (d as HamburgerMenu).InternalDisplayModeChanged(e.ToChangedEventArgs<SplitViewDisplayMode>());
                }));
        public event EventHandler<ChangedEventArgs<SplitViewDisplayMode>> DisplayModeChanged;
        partial void InternalDisplayModeChanged(ChangedEventArgs<SplitViewDisplayMode> e);

        #endregion

        #region Binding Properties

        /// <summary>
        /// 获取或设置导航栏是否展开
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool),
                typeof(HamburgerMenu), new PropertyMetadata(false, (d, e) =>
                {
                    WriteDebug(nameof(IsOpen), e);
                    (d as HamburgerMenu).IsOpenChanged?.Invoke(d, e.ToChangedEventArgs<bool>());
                    (d as HamburgerMenu).InternalIsOpenChanged(e.ToChangedEventArgs<bool>());
                }));
        /// <summary>
        /// 在导航栏展开状态改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<bool>> IsOpenChanged;
        partial void InternalIsOpenChanged(ChangedEventArgs<bool> e);


        /// <summary>
        /// 获取或设置导航栏展开后的宽度
        /// </remarks>
        public double PaneWidth
        {
            get { return (double)GetValue(PaneWidthProperty); }
            set { SetValue(PaneWidthProperty, value); }
        }
        public static readonly DependencyProperty PaneWidthProperty =
            DependencyProperty.Register(nameof(PaneWidth), typeof(double),
                typeof(HamburgerMenu), new PropertyMetadata(220d, (d, e) =>
                {
                    WriteDebug(nameof(PaneWidth), e);
                    (d as HamburgerMenu).PaneWidthChanged?.Invoke(d, e.ToChangedEventArgs<double>());
                    (d as HamburgerMenu).InternalPaneWidthChanged(e.ToChangedEventArgs<double>());
                }));
        /// <summary>
        /// 在导航栏展开后宽度改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<double>> PaneWidthChanged;
        partial void InternalPaneWidthChanged(ChangedEventArgs<double> e);


        /// <summary>
        /// 指定导航栏展开的操作方式
        /// </summary>
        [Flags]
        public enum OpenCloseModes {
            None = 1,
            /// <summary>
            /// 自动设置
            /// </summary>
            Auto = 2,
            /// <summary>
            /// 双击两次汉堡菜单空白区域
            /// </summary>
            Tap = 4,
            /// <summary>
            /// 向右滑动展开，向左滑动收起
            /// </summary>
            Swipe = 5 }
        /// <summary>
        /// 获取或设置使汉堡菜单导航栏展开的操作方法
        /// </remarks>
        public OpenCloseModes OpenCloseMode
        {
            get { return (OpenCloseModes)GetValue(OpenCloseModeProperty); }
            set { SetValue(OpenCloseModeProperty, value); }
        }
        public static readonly DependencyProperty OpenCloseModeProperty =
            DependencyProperty.Register(nameof(OpenCloseMode), typeof(OpenCloseModes),
                typeof(HamburgerMenu), new PropertyMetadata(OpenCloseModes.Auto, (d, e) =>
                {
                    WriteDebug(nameof(OpenCloseMode), e);
                    (d as HamburgerMenu).OpenCloseModeChanged?.Invoke(d, e.ToChangedEventArgs<OpenCloseModes>());
                    (d as HamburgerMenu).InternalOpenCloseModeChanged(e.ToChangedEventArgs<OpenCloseModes>());
                }));
        public event EventHandler<ChangedEventArgs<OpenCloseModes>> OpenCloseModeChanged;
        partial void InternalOpenCloseModeChanged(ChangedEventArgs<OpenCloseModes> e);


        /// <summary>
        /// 获取或设置汉堡菜单的内容Grid是否全屏展示（导航栏和汉堡按钮是否隐藏）
        /// </summary>
        /// <example>
        /// 在播放媒体内容时需要全屏可以使用这个属性
        /// </example>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register(nameof(IsFullScreen), typeof(bool),
                typeof(HamburgerMenu), new PropertyMetadata(false, (d, e) =>
                {
                    WriteDebug(nameof(IsFullScreen), e);
                    (d as HamburgerMenu).IsFullScreenChanged?.Invoke(d, e.ToChangedEventArgs<bool>());
                    (d as HamburgerMenu).InternalIsFullScreenChanged(e.ToChangedEventArgs<bool>());
                }));
        public event EventHandler<ChangedEventArgs<bool>> IsFullScreenChanged;
        partial void InternalIsFullScreenChanged(ChangedEventArgs<bool> e);


        /// <summary>
        /// 获取或设置当导航栏展开时二级按钮的排列方向。该属性仅在<see cref="IsOpen"/>属性为真且<see cref="DisplayMode"/>不为Compact时生效。
        /// </summary>
        /// <remarks>
        /// 在Compact Mode下，二级按钮按竖直排列，因为其他的内容没有实际意义
        /// </remarks>
        public Orientation SecondaryButtonOrientation
        {
            get { return (Orientation)GetValue(SecondaryButtonOrientationProperty); }
            set { SetValue(SecondaryButtonOrientationProperty, value); }
        }
        public static readonly DependencyProperty SecondaryButtonOrientationProperty =
            DependencyProperty.Register(nameof(SecondaryButtonOrientation), typeof(Orientation),
                typeof(HamburgerMenu), new PropertyMetadata(Orientation.Vertical, (d, e) =>
                {
                    (d as HamburgerMenu).UpdateSecondaryButtonOrientation();
                    WriteDebug(nameof(SecondaryButtonOrientation), e);
                    (d as HamburgerMenu).SecondaryButtonOrientationChanged?.Invoke(d, e.ToChangedEventArgs<Orientation>());
                    (d as HamburgerMenu).InternalSecondaryButtonOrientationChanged(e.ToChangedEventArgs<Orientation>());
                }));
        public event EventHandler<ChangedEventArgs<Orientation>> SecondaryButtonOrientationChanged;
        partial void InternalSecondaryButtonOrientationChanged(ChangedEventArgs<Orientation> e);

        /// <summary>
        /// 获取或设置当前高亮的导航栏中的按钮，设置该属性会触发导航行为
        /// </summary>
        public HamburgerButtonInfo Selected
        {
            get { return GetValue(SelectedProperty) as HamburgerButtonInfo; }
            set { SetValue(SelectedProperty, value); }
        }
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register(nameof(Selected), typeof(HamburgerButtonInfo),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
                {
                    WriteDebug(nameof(Selected), e);
                    (d as HamburgerMenu).SelectedChanged?.Invoke(d, e.ToChangedEventArgs<HamburgerButtonInfo>());
                    (d as HamburgerMenu).InternalSelectedChanged(e.ToChangedEventArgs<HamburgerButtonInfo>());
                }));
        public event EventHandler<ChangedEventArgs<HamburgerButtonInfo>> SelectedChanged;
        partial void InternalSelectedChanged(ChangedEventArgs<HamburgerButtonInfo> e);

        #endregion

    }
}
