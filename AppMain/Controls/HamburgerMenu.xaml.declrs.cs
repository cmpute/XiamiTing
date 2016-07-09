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
    public sealed partial class HamburgerMenu : UserControl
    {

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
                    WriteDebug(nameof(DisplayMode), e);
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
                    WriteDebug(nameof(IsOpen), e);
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
                    WriteDebug(nameof(PaneWidth), e);
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
                    WriteDebug(nameof(OpenCloseMode), e);
                    (d as HamburgerMenu).OpenCloseModeChanged?.Invoke(d, e.ToChangedEventArgs<OpenCloseModes>());
                    (d as HamburgerMenu).InternalOpenCloseModeChanged(e.ToChangedEventArgs<OpenCloseModes>());
                }));
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
                    WriteDebug(nameof(IsFullScreen), e);
                    (d as HamburgerMenu).IsFullScreenChanged?.Invoke(d, e.ToChangedEventArgs<bool>());
                    (d as HamburgerMenu).InternalIsFullScreenChanged(e.ToChangedEventArgs<bool>());
                }));
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
                    WriteDebug(nameof(SecondaryButtonOrientation), e);
                    (d as HamburgerMenu).SecondaryButtonOrientationChanged?.Invoke(d, e.ToChangedEventArgs<Orientation>());
                    (d as HamburgerMenu).InternalSecondaryButtonOrientationChanged(e.ToChangedEventArgs<Orientation>());
                }));
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
                    WriteDebug(nameof(Selected), e);
                    (d as HamburgerMenu).SelectedChanged?.Invoke(d, e.ToChangedEventArgs<HamburgerButtonInfo>());
                    (d as HamburgerMenu).InternalSelectedChanged(e.ToChangedEventArgs<HamburgerButtonInfo>());
                }));
        public event EventHandler<ChangedEventArgs<HamburgerButtonInfo>> SelectedChanged;
        partial void InternalSelectedChanged(ChangedEventArgs<HamburgerButtonInfo> e);

        #endregion

    }
}
