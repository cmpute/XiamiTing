using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
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

        #region Declaration Implemetations

        private void UpdateSecondaryButtonOrientation()
        {
            if (_SecondaryButtonStackPanel == null) return;

            // secondary layout
            if (SecondaryButtonOrientation.Equals(Orientation.Horizontal) && IsOpen)
            {
                _SecondaryButtonStackPanel.Orientation = Orientation.Horizontal;
            }
            else
            {
                _SecondaryButtonStackPanel.Orientation = Orientation.Vertical;
            }
        }

        //Hamburger的动作Command
        private void ExecuteHamburger()
        {
            DebugWrite();
            IsOpen = !IsOpen;
        }

        //TODO: 待读懂
        public class InfoElement
        {
            public InfoElement(object sender)
            {
                FrameworkElement = sender as FrameworkElement;
                HamburgerButtonInfo = FrameworkElement?.DataContext as HamburgerButtonInfo;
            }
            public T GetElement<T>() where T : DependencyObject => FrameworkElement as T;

            public void RefreshVisualState()
            {
                var children = FrameworkElement.AllChildren();
                var child = children.OfType<Grid>().First(x => x.Name == "RootGrid");
                var groups = VisualStateManager.GetVisualStateGroups(child);
                var group = groups.First(x => x.Name == "CommonStates");
                var current = group.CurrentState.Name;
                VisualStateManager.GoToState(GetElement<Control>(), "Indeterminate", false);
                VisualStateManager.GoToState(GetElement<Control>(), current, false);
            }

            public FrameworkElement FrameworkElement { get; }
            public Button Button => GetElement<Button>();
            public ToggleButton ToggleButton => GetElement<ToggleButton>();
            public HamburgerButtonInfo HamburgerButtonInfo { get; }
        }

        private void PaneContent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DebugWrite($"OpenCloseMode {OpenCloseMode}");

            if (DisplayMode == SplitViewDisplayMode.CompactInline || DisplayMode == SplitViewDisplayMode.Inline)
            {
                return;
            }
            var button = new InfoElement(e.OriginalSource);
            if (button.HamburgerButtonInfo?.IsChecked ?? false)
            {
                return;
            }

            switch (OpenCloseMode)
            {
                case OpenCloseModes.Auto:
                case OpenCloseModes.Tap:
                    HamburgerCommand.Execute(null);
                    break;
            }
        }

        private void PaneContent_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            DebugWrite($"OpenCloseMode {OpenCloseMode}");

            if (e.PointerDeviceType == PointerDeviceType.Mouse) return;
            if (e.PointerDeviceType == PointerDeviceType.Pen) return;
            switch (OpenCloseMode)
            {
                case OpenCloseModes.None:
                case OpenCloseModes.Tap:
                    return;
            }

            var threshold = 24;
            var delta = e.Cumulative.Translation.X;
            if (delta < -threshold) IsOpen = false;
            else if (delta > threshold) IsOpen = true;
        }

        #endregion

        private StackPanel _SecondaryButtonStackPanel;
        private void SecondaryButtonStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _SecondaryButtonStackPanel = sender as StackPanel;
            UpdateSecondaryButtonOrientation();
        }
    }
}
