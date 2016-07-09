using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Command;
using JacobC;

namespace JacobC.Controls
{
    public sealed partial class HamburgerMenu : UserControl
    {

        #region Private Members

        private static void DebugWrite(string text, DependencyPropertyChangedEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now.TimeOfDay.ToString()} OldValue: {e.OldValue} NewValue: {e.NewValue}] {text}");
#endif
        }
        private static void DebugWrite(string text = null, [CallerMemberName]string caller = null)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now.TimeOfDay.ToString()} Caller: {caller}] {text}");
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
                    DebugWrite(nameof(DisplayMode), e);
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
                    DebugWrite(nameof(IsOpen), e);
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
                    DebugWrite(nameof(PaneWidth), e);
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
        public enum OpenCloseModes
        {
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
            Swipe = 5
        }
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
                    DebugWrite(nameof(OpenCloseMode), e);
                    (d as HamburgerMenu).OpenCloseModeChanged?.Invoke(d, e.ToChangedEventArgs<OpenCloseModes>());
                    (d as HamburgerMenu).InternalOpenCloseModeChanged(e.ToChangedEventArgs<OpenCloseModes>());
                }));
        /// <summary>
        /// 在汉堡菜单导航栏展开的操作方法发生变化时发生
        /// </summary>
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
                    DebugWrite(nameof(IsFullScreen), e);
                    (d as HamburgerMenu).IsFullScreenChanged?.Invoke(d, e.ToChangedEventArgs<bool>());
                    (d as HamburgerMenu).InternalIsFullScreenChanged(e.ToChangedEventArgs<bool>());
                }));
        /// <summary>
        /// 在汉堡菜单进入或退出FullScreen时发生
        /// </summary>
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
                    DebugWrite(nameof(SecondaryButtonOrientation), e);
                    (d as HamburgerMenu).SecondaryButtonOrientationChanged?.Invoke(d, e.ToChangedEventArgs<Orientation>());
                    (d as HamburgerMenu).InternalSecondaryButtonOrientationChanged(e.ToChangedEventArgs<Orientation>());
                }));
        /// <summary>
        /// 在二级按钮排列方向改变时发生
        /// </summary>
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
                    DebugWrite(nameof(Selected), e);
                    (d as HamburgerMenu).SelectedChanged?.Invoke(d, e.ToChangedEventArgs<HamburgerButtonInfo>());
                    (d as HamburgerMenu).InternalSelectedChanged(e.ToChangedEventArgs<HamburgerButtonInfo>());
                }));
        /// <summary>
        /// 在更改选中高亮的导航按钮时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<HamburgerButtonInfo>> SelectedChanged;
        partial void InternalSelectedChanged(ChangedEventArgs<HamburgerButtonInfo> e);


        /// <summary>
        /// 获取或设置导航栏和内容页面间边框的厚度
        /// </summary>
        public Thickness PaneBorderThickness
        {
            get { return (Thickness)GetValue(PaneBorderThicknessProperty); }
            set { SetValue(PaneBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty PaneBorderThicknessProperty =
            DependencyProperty.Register(nameof(PaneBorderThickness), typeof(Thickness),
                typeof(HamburgerMenu), new PropertyMetadata(new Thickness(0, 0, 1, 0), (d, e) =>
                {
                    DebugWrite(nameof(PaneBorderThickness), e);
                    (d as HamburgerMenu).PaneBorderThicknessChanged?.Invoke(d, e.ToChangedEventArgs<Thickness>());
                    (d as HamburgerMenu).InternalPaneBorderThicknessChanged(e.ToChangedEventArgs<Thickness>());
                }));
        /// <summary>
        /// 在导航栏边框厚度变更时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<Thickness>> PaneBorderThicknessChanged;
        partial void InternalPaneBorderThicknessChanged(ChangedEventArgs<Thickness> e);


        /// <summary>
        /// 获取或设置汉堡导航按钮的可见性
        /// </summary>
        public Visibility HamburgerButtonVisibility
        {
            get { return (Visibility)GetValue(HamburgerButtonVisibilityProperty); }
            set { SetValue(HamburgerButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty HamburgerButtonVisibilityProperty =
            DependencyProperty.Register(nameof(HamburgerButtonVisibility), typeof(Visibility),
                typeof(HamburgerMenu), new PropertyMetadata(Visibility.Visible, (d, e) =>
                {
                    DebugWrite(nameof(HamburgerButtonVisibility), e);
                    (d as HamburgerMenu).HamburgerButtonVisibilityChanged?.Invoke(d, e.ToChangedEventArgs<Visibility>());
                    (d as HamburgerMenu).InternalHamburgerButtonVisibilityChanged(e.ToChangedEventArgs<Visibility>());
                }));
        public event EventHandler<ChangedEventArgs<Visibility>> HamburgerButtonVisibilityChanged;
        partial void InternalHamburgerButtonVisibilityChanged(ChangedEventArgs<Visibility> e);


        /// <summary>
        /// 获取或设置标题栏的内容元素
        /// </summary>
        public UIElement HeaderContent
        {
            get { return (UIElement)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register(nameof(HeaderContent), typeof(UIElement),
         typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
         {
             DebugWrite(nameof(HeaderContent), e);
             (d as HamburgerMenu).HeaderContentChanged?.Invoke(d, e.ToChangedEventArgs<UIElement>());
             (d as HamburgerMenu).InternalHeaderContentChanged(e.ToChangedEventArgs<UIElement>());
         }));
        /// <summary>
        /// 在标题栏的内容元素更改时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<UIElement>> HeaderContentChanged;
        partial void InternalHeaderContentChanged(ChangedEventArgs<UIElement> e);


        /// <summary>
        /// 获取或设置导航栏的一级按钮（顶端）
        /// SecondaryButtons are the button at the top of the HamburgerMenu
        /// </summary>
        public ObservableCollection<HamburgerButtonInfo> PrimaryButtons
        {
            get { return (ObservableCollection<HamburgerButtonInfo>)base.GetValue(PrimaryButtonsProperty); }
            set { SetValue(PrimaryButtonsProperty, value); }
        }
        public static readonly DependencyProperty PrimaryButtonsProperty =
            DependencyProperty.Register(nameof(PrimaryButtons), typeof(ObservableCollection<HamburgerButtonInfo>),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
                {
                    DebugWrite(nameof(PrimaryButtons), e);
                }));


        /// <summary>
        /// 获取或设置导航栏的二级按钮（底端）
        /// </summary>
        public ObservableCollection<HamburgerButtonInfo> SecondaryButtons
        {
            get { return (ObservableCollection<HamburgerButtonInfo>)base.GetValue(SecondaryButtonsProperty); }
            set { SetValue(SecondaryButtonsProperty, value); }
        }
        public static readonly DependencyProperty SecondaryButtonsProperty =
            DependencyProperty.Register(nameof(SecondaryButtons), typeof(ObservableCollection<HamburgerButtonInfo>),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
                {
                    DebugWrite(nameof(SecondaryButtons), e);
                }));


        #endregion

        #region Style Properties

        /// <summary>
        /// 获取或设置导航栏边框的画笔
        /// </summary>
        public Brush PaneBorderBrush
        {
            get { return GetValue(PaneBorderBrushProperty) as Brush; }
            set { SetValue(PaneBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty PaneBorderBrushProperty =
              DependencyProperty.Register(nameof(PaneBorderBrush), typeof(Brush),
                  typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
                  {
                      DebugWrite(nameof(PaneBorderBrush), e);
                      (d as HamburgerMenu).PaneBorderBrushChanged?.Invoke(d, e.ToChangedEventArgs<Brush>());
                      (d as HamburgerMenu).InternalPaneBorderBrushChanged(e.ToChangedEventArgs<Brush>());
                  }));
        /// <summary>
        /// 在导航栏边框画笔改变时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<Brush>> PaneBorderBrushChanged;
        partial void InternalPaneBorderBrushChanged(ChangedEventArgs<Brush> e);


        /// <summary>
        /// 获取或设置二级按钮分割线的画笔
        /// </summary>
        public Brush SecondarySeparator
        {
            get { return GetValue(SecondarySeparatorProperty) as Brush; }
            set { SetValue(SecondarySeparatorProperty, value); }
        }
        public static readonly DependencyProperty SecondarySeparatorProperty =
              DependencyProperty.Register(nameof(SecondarySeparator), typeof(Brush),
                  typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
                  {
                      DebugWrite(nameof(SecondarySeparator), e);
                      (d as HamburgerMenu).SecondarySeparatorChanged?.Invoke(d, e.ToChangedEventArgs<Brush>());
                      (d as HamburgerMenu).InternalSecondarySeparatorChanged(e.ToChangedEventArgs<Brush>());
                  }));
        /// <summary>
        /// 在<see cref="SecondarySeparator"/>属性发生更改时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<Brush>> SecondarySeparatorChanged;
        partial void InternalSecondarySeparatorChanged(ChangedEventArgs<Brush> e);

        /// <summary>
        /// 获取或设置汉堡菜单的前景色
        /// </summary>
        public Brush HamburgerForeground
        {
            get { return GetValue(HamburgerForegroundProperty) as Brush; }
            set { SetValue(HamburgerForegroundProperty, value); }
        }
        private static readonly Brush _defaultHamburgerForeground = Colors.White.ToSolidColorBrush();
        public static readonly DependencyProperty HamburgerForegroundProperty =
              DependencyProperty.Register(nameof(HamburgerForeground), typeof(Brush),
                  typeof(HamburgerMenu), new PropertyMetadata(_defaultHamburgerForeground, (d, e) =>
                  {
                      DebugWrite(nameof(HamburgerForeground), e);
                      (d as HamburgerMenu).HamburgerForegroundChanged?.Invoke(d, e.ToChangedEventArgs<Brush>());
                      (d as HamburgerMenu).InternalHamburgerForegroundChanged(e.ToChangedEventArgs<Brush>());
                  }));
        /// <summary>
        /// 在<see cref="HamburgerForeground"/>属性发生更改时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<Brush>> HamburgerForegroundChanged;
        partial void InternalHamburgerForegroundChanged(ChangedEventArgs<Brush> e);


        /// <summary>
        /// 获取或设置汉堡菜单的背景色
        /// </summary>
        public Brush HamburgerBackground
        {
            get { return GetValue(HamburgerBackgroundProperty) as Brush; }
            set { SetValue(HamburgerBackgroundProperty, value); }
        }
        private static readonly Brush _defaultHamburgerBackground = Colors.SteelBlue.ToSolidColorBrush();
        public static readonly DependencyProperty HamburgerBackgroundProperty =
            DependencyProperty.Register(nameof(HamburgerBackground), typeof(Brush),
                typeof(HamburgerMenu), new PropertyMetadata(_defaultHamburgerBackground, (d, e) =>
                {
                    DebugWrite(nameof(HamburgerBackground), e);
                    (d as HamburgerMenu).HamburgerBackgroundChanged?.Invoke(d, e.ToChangedEventArgs<Brush>());
                    (d as HamburgerMenu).InternalHamburgerBackgroundChanged(e.ToChangedEventArgs<Brush>());
                }));
        /// <summary>
        /// 在<see cref="HamburgerBackground"/>属性发生更改时发生
        /// </summary>
        public event EventHandler<ChangedEventArgs<Brush>> HamburgerBackgroundChanged;
        partial void InternalHamburgerBackgroundChanged(ChangedEventArgs<Brush> e);


        #endregion

        #region Command
        RelayCommand _hamburgerCommand;
        internal RelayCommand HamburgerCommand => _hamburgerCommand ?? (_hamburgerCommand = new RelayCommand(ExecuteHamburger));


        #endregion
    }
}
