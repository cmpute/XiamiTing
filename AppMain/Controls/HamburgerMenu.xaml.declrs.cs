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
        /// ������Ϊ�ڲ�SpiltView��DisplayMode��ͶӰ����
        /// </summary>
        /// <remarks>
        /// ������һ����VisualState�Զ����ã���Ҳ�����ڶ��ƿؼ�ʱ�ֶ�����
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
        /// ��ȡ�����õ������Ƿ�չ��
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
        /// �ڵ�����չ��״̬�ı�ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<bool>> IsOpenChanged;
        partial void InternalIsOpenChanged(ChangedEventArgs<bool> e);


        /// <summary>
        /// ��ȡ�����õ�����չ����Ŀ��
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
        /// �ڵ�����չ�����ȸı�ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<double>> PaneWidthChanged;
        partial void InternalPaneWidthChanged(ChangedEventArgs<double> e);


        /// <summary>
        /// ָ��������չ���Ĳ�����ʽ
        /// </summary>
        [Flags]
        public enum OpenCloseModes
        {
            None = 1,
            /// <summary>
            /// �Զ�����
            /// </summary>
            Auto = 2,
            /// <summary>
            /// ˫�����κ����˵��հ�����
            /// </summary>
            Tap = 4,
            /// <summary>
            /// ���һ���չ�������󻬶�����
            /// </summary>
            Swipe = 5
        }
        /// <summary>
        /// ��ȡ������ʹ�����˵�������չ���Ĳ�������
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
        /// �ں����˵�������չ���Ĳ������������仯ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<OpenCloseModes>> OpenCloseModeChanged;
        partial void InternalOpenCloseModeChanged(ChangedEventArgs<OpenCloseModes> e);


        /// <summary>
        /// ��ȡ�����ú����˵�������Grid�Ƿ�ȫ��չʾ���������ͺ�����ť�Ƿ����أ�
        /// </summary>
        /// <example>
        /// �ڲ���ý������ʱ��Ҫȫ������ʹ���������
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
        /// �ں����˵�������˳�FullScreenʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<bool>> IsFullScreenChanged;
        partial void InternalIsFullScreenChanged(ChangedEventArgs<bool> e);


        /// <summary>
        /// ��ȡ�����õ�������չ��ʱ������ť�����з��򡣸����Խ���<see cref="IsOpen"/>����Ϊ����<see cref="DisplayMode"/>��ΪCompactʱ��Ч��
        /// </summary>
        /// <remarks>
        /// ��Compact Mode�£�������ť����ֱ���У���Ϊ����������û��ʵ������
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
        /// �ڶ�����ť���з���ı�ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<Orientation>> SecondaryButtonOrientationChanged;
        partial void InternalSecondaryButtonOrientationChanged(ChangedEventArgs<Orientation> e);


        /// <summary>
        /// ��ȡ�����õ�ǰ�����ĵ������еİ�ť�����ø����Իᴥ��������Ϊ
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
        /// �ڸ���ѡ�и����ĵ�����ťʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<HamburgerButtonInfo>> SelectedChanged;
        partial void InternalSelectedChanged(ChangedEventArgs<HamburgerButtonInfo> e);


        /// <summary>
        /// ��ȡ�����õ�����������ҳ���߿�ĺ��
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
        /// �ڵ������߿��ȱ��ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<Thickness>> PaneBorderThicknessChanged;
        partial void InternalPaneBorderThicknessChanged(ChangedEventArgs<Thickness> e);


        /// <summary>
        /// ��ȡ�����ú���������ť�Ŀɼ���
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
        /// ��ȡ�����ñ�����������Ԫ��
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
        /// �ڱ�����������Ԫ�ظ���ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<UIElement>> HeaderContentChanged;
        partial void InternalHeaderContentChanged(ChangedEventArgs<UIElement> e);


        /// <summary>
        /// ��ȡ�����õ�������һ����ť�����ˣ�
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
        /// ��ȡ�����õ������Ķ�����ť���׶ˣ�
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
        /// ��ȡ�����õ������߿�Ļ���
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
        /// �ڵ������߿򻭱ʸı�ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<Brush>> PaneBorderBrushChanged;
        partial void InternalPaneBorderBrushChanged(ChangedEventArgs<Brush> e);


        /// <summary>
        /// ��ȡ�����ö�����ť�ָ��ߵĻ���
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
        /// ��<see cref="SecondarySeparator"/>���Է�������ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<Brush>> SecondarySeparatorChanged;
        partial void InternalSecondarySeparatorChanged(ChangedEventArgs<Brush> e);

        /// <summary>
        /// ��ȡ�����ú����˵���ǰ��ɫ
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
        /// ��<see cref="HamburgerForeground"/>���Է�������ʱ����
        /// </summary>
        public event EventHandler<ChangedEventArgs<Brush>> HamburgerForegroundChanged;
        partial void InternalHamburgerForegroundChanged(ChangedEventArgs<Brush> e);


        /// <summary>
        /// ��ȡ�����ú����˵��ı���ɫ
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
        /// ��<see cref="HamburgerBackground"/>���Է�������ʱ����
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
