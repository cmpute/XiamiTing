using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static JacobC.ExtensionMethods;
using System.Threading.Tasks;
using Windows.System;
using System.Diagnostics;
using System.Windows.Input;

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
            DebugWrite();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                InitializeComponent();
            }
            else
            {

                PrimaryButtons = new ObservableCollection<HamburgerButtonInfo>();
                SecondaryButtons = new ObservableCollection<HamburgerButtonInfo>();

                InitializeComponent();

                Loaded += HamburgerMenu_Loaded;
                LayoutUpdated += HamburgerMenu_LayoutUpdated;

#if DEBUG
                GotFocus += (s, e) => //Debug用
                {
                    var element = FocusManager.GetFocusedElement() as FrameworkElement;
                    var name = element?.Name ?? "no-name";
                    var stackpanel = (element as ContentControl)?.Content as StackPanel;
                    var symbolicon = stackpanel?.Children[0] as SymbolIcon;
                    var symbol = symbolicon?.Symbol.ToString();
                    symbol = symbol ?? (element as ContentControl)?.Content?.ToString() ?? "no-content";
                    var value = $"{element?.ToString() ?? "null"} name:{name} symbol:{symbol}";
                    DebugWrite(value, caller: "GotFocus");
                };
#endif
            }

        }

        #region HamburgerMenu Controls Event

        private void HamburgerMenu_Loaded(object sender, RoutedEventArgs args)
        {
            DebugWrite();

            // non-custom property changes
            ShellSplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, SplitView_IsPaneOpenChanged);
            ShellSplitView.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, SplitView_DisplayModeChanged);
            RegisterPropertyChangedCallback(RequestedThemeProperty, HamburgerMenu_RequestedThemeChanged);

            // 增加按键导航
            HamburgerButton.KeyDown += HamburgerMenu_KeyDown;
            PrimaryButtonContainer.KeyDown += HamburgerMenu_KeyDown;
            SecondaryButtonContainer.KeyDown += HamburgerMenu_KeyDown;

            // 初始化样式
            UpdateHamburgerButtonGridWidthToFillAnyGap();
            RefreshStyles(RequestedTheme);
            UpdatePaneMarginToShowHamburgerButton();
        }
        private void HamburgerMenu_RequestedThemeChanged(DependencyObject sender, DependencyProperty dp)
        {
            DebugWrite();
            RefreshStyles(RequestedTheme);
        }
        private void HamburgerMenu_LayoutUpdated(object sender, object e)
        {
            DebugWrite();
            LayoutUpdated -= HamburgerMenu_LayoutUpdated;
            UpdateVisualStates();
            UpdateControl();
        }
        // 处理按键事件的导航
        private void HamburgerMenu_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var currentItem = FocusManager.GetFocusedElement() as FrameworkElement;
            var lastItem = LoadedNavButtons.FirstOrDefault(x => x.HamburgerButtonInfo == (SecondaryButtons.LastOrDefault(a => a != Selected) ?? PrimaryButtons.LastOrDefault(a => a != Selected)));

            var focus = new Func<FocusNavigationDirection, bool>(d =>
            {
                if (d == FocusNavigationDirection.Next)
                {
                    return FocusManager.TryMoveFocus(d);
                }
                else if (d == FocusNavigationDirection.Previous)
                {
                    return FocusManager.TryMoveFocus(d);
                }
                else
                {
                    var control = FocusManager.FindNextFocusableElement(d) as Control;
                    return control?.Focus(FocusState.Programmatic) ?? false;
                }
            });

            var escape = new Func<bool>(() =>
            {
                if (DisplayMode == SplitViewDisplayMode.CompactOverlay
                    || DisplayMode == SplitViewDisplayMode.Overlay)
                    IsOpen = false;
                if (Equals(ShellSplitView.PanePlacement, SplitViewPanePlacement.Left))
                {
                    ShellSplitView.Content.RenderTransform = new TranslateTransform { X = 48 + ShellSplitView.OpenPaneLength };
                    focus(FocusNavigationDirection.Right);
                    ShellSplitView.Content.RenderTransform = null;
                }
                else
                {
                    ShellSplitView.Content.RenderTransform = new TranslateTransform { X = -48 - ShellSplitView.OpenPaneLength };
                    focus(FocusNavigationDirection.Left);
                    ShellSplitView.Content.RenderTransform = null;
                }
                return true;
            });

            var previous = new Func<bool>(() =>
            {
                if (Equals(currentItem, HamburgerButton))
                {
                    return true;
                }
                else if (focus(FocusNavigationDirection.Previous) || focus(FocusNavigationDirection.Up))
                {
                    return true;
                }
                else
                {
                    return escape();
                }
            });

            var next = new Func<bool>(() =>
            {
                if (Equals(currentItem, HamburgerButton))
                {
                    return focus(FocusNavigationDirection.Down);
                }
                else if (focus(FocusNavigationDirection.Next) || focus(FocusNavigationDirection.Down))
                {
                    return true;
                }
                else
                {
                    return escape();
                }
            });

            if (IsFullScreen)
            {
                return;
            }

            switch (e.Key)
            {
                case VirtualKey.Up:
                case VirtualKey.GamepadDPadUp:

                    if (!(e.Handled = previous())) Debugger.Break();
                    break;

                case VirtualKey.Down:
                case VirtualKey.GamepadDPadDown:

                    if (!(e.Handled = next())) Debugger.Break();
                    break;

                case VirtualKey.Right:
                case VirtualKey.GamepadDPadRight:
                    if (SecondaryButtonContainer.Items.Contains(currentItem?.DataContext)
                        && SecondaryButtonOrientation == Orientation.Horizontal)
                    {
                        if (Equals(lastItem.FrameworkElement, currentItem))
                        {
                            if (!(e.Handled = escape())) Debugger.Break();
                        }
                        else
                        {
                            if (!(e.Handled = next())) Debugger.Break();
                        }
                    }
                    else
                    {
                        if (!(e.Handled = escape())) Debugger.Break();
                    }
                    break;

                case VirtualKey.Left:
                case VirtualKey.GamepadDPadLeft:

                    if (SecondaryButtonContainer.Items.Contains(currentItem?.DataContext)
                       && SecondaryButtonOrientation == Orientation.Horizontal)
                    {
                        if (Equals(lastItem.FrameworkElement, currentItem))
                        {
                            if (!(e.Handled = escape())) Debugger.Break();
                        }
                        else
                        {
                            if (!(e.Handled = previous())) Debugger.Break();
                        }
                    }
                    else
                    {
                        if (!(e.Handled = escape())) Debugger.Break();
                    }
                    break;

                case VirtualKey.Space:
                case VirtualKey.Enter:
                case VirtualKey.GamepadA:

                    if (currentItem != null)
                    {
                        var info = new InfoElement(currentItem);
                        NavCommand.Execute(info.HamburgerButtonInfo);
                    }

                    break;

                case VirtualKey.Escape:
                case VirtualKey.GamepadB:

                    if (!(e.Handled = escape())) Debugger.Break();
                    break;
            }
        }

        private void SplitView_DisplayModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (DisplayMode != ShellSplitView.DisplayMode)
                DisplayMode = ShellSplitView.DisplayMode;
        }
        private void SplitView_IsPaneOpenChanged(DependencyObject sender, DependencyProperty dp)
        {
            // 方法会在窗口加载前改变大小时发生this can occur if the user resizes before it loads
            if (_SecondaryButtonStackPanel == null)
                return;

            UpdateSecondaryButtonOrientation();

            // 保证属性同步this will keep the two properties in sync
            if (IsOpen != ShellSplitView.IsPaneOpen)
                IsOpen = ShellSplitView.IsPaneOpen;

            UpdateHamburgerButtonGridWidthToFillAnyGap();
        }

        #endregion

        private void RefreshStyles(ElementTheme theme)
        {
            //TODO:增加更换主题颜色的处理
        }

        private void VisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateVisualStates();
            if (IsFullScreen)
            {
                UpdateFullScreen();
            }
        }
        
        #region Partial Property Changed Handlers Implementation

        partial void InternalHeaderContentChanged(ChangedEventArgs<UIElement> e) => UpdatePaneMarginToShowHamburgerButton();
        partial void InternalIsFullScreenChanged(ChangedEventArgs<bool> e) => UpdateControl(e.NewValue);
        partial void InternalIsOpenChanged(ChangedEventArgs<bool> e)
        {
            UpdateIsPaneOpen(e.NewValue);
            UpdateHamburgerButtonGridWidthToFillAnyGap();
            UpdateControl();
        }
        partial void InternalHamburgerButtonVisibilityChanged(ChangedEventArgs<Visibility> e)
        {
            HamburgerButton.Visibility = e.NewValue;
            UpdatePaneMarginToShowHamburgerButton();
        }
        async partial void InternalSelectedChanged(ChangedEventArgs<HamburgerButtonInfo> e)
        {
            if ((e.NewValue?.Equals(e.OldValue) ?? false))
            {
                e.NewValue.IsChecked = (e.NewValue.ButtonType == HamburgerButtonInfo.ButtonTypes.Toggle);
            }

            try
            {
                await UpdateSelectedAsync(e.OldValue, e.NewValue);
            }
            catch (Exception ex)
            {
                DebugWrite($"Catch Ex.Message: {ex.Message}", caller: "SelectedPropertyChanged");
            }
        }

        //partial void InternalNavigationServiceChanged(ChangedEventArgs<INavigationService> e)
        //{
        //    e.NewValue.AfterRestoreSavedNavigation += (s, args) => HighlightCorrectButton(NavigationService.CurrentPageType, NavigationService.CurrentPageParam);
        //    e.NewValue.FrameFacade.Navigated += (s, args) => HighlightCorrectButton(args.PageType, args.Parameter);
        //    ShellSplitView.Content = e.NewValue.Frame;
        //    UpdateFullScreenForSplashScreen(e);
        //}

        #endregion

        #region Update methods

        //private void UpdateFullScreenForSplashScreen(ChangedEventArgs<INavigationService> e)
        //{
        //    // If splash screen then continue showing until navigated once
        //    if (e.NewValue.FrameFacade.BackStackDepth == 0
        //        && e.NewValue.Frame.Content != null
        //        && App.Current.SplashFactory != null
        //        && BootStrapper.Current.OriginalActivatedArgs.PreviousExecutionState != Windows.ApplicationModel.Activation.ApplicationExecutionState.Terminated)
        //    {
        //        var once = false;
        //        UpdateControl(true);
        //        e.NewValue.FrameFacade.Navigated += (s, args) =>
        //        {
        //            if (!once)
        //            {
        //                once = true;
        //                if (!IsFullScreen)
        //                {
        //                    UpdateControl(false);
        //                }
        //            }
        //        };
        //    }
        //}

        internal void HighlightCorrectButton(Type pageType, object pageParam)
        {
            DebugWrite($"PageType: {pageType} PageParam: {pageParam}");

            // match type only
            var buttons = LoadedNavButtons.Where(x => Equals(x.HamburgerButtonInfo.PageType, pageType));

            // serialize parameter for matching
            //if (pageParam == null)
            //{
            //    pageParam = NavigationService.CurrentPageParam;
            //}
            //else if (pageParam.ToString().StartsWith("{"))
            //{
            //    try
            //    {
            //        pageParam = NavigationService.FrameFacade.SerializationService.Deserialize(pageParam.ToString());
            //    }
            //    catch { }
            //}

            // add parameter match
            buttons = buttons.Where(x => Equals(x.HamburgerButtonInfo.PageParameter, null) || Equals(x.HamburgerButtonInfo.PageParameter, pageParam));
            var button = buttons.Select(x => x.HamburgerButtonInfo).FirstOrDefault();
            Selected = button;
        }

        private async Task ResetValueAsync(DependencyProperty prop, object tempValue, int wait = 50)
        {
            if (GetValue(prop) == DependencyProperty.UnsetValue)
            {
                return;
            }
            var original = GetValue(prop);
            SetValue(prop, tempValue);
            await Task.Delay(wait);
            SetValue(prop, original);
        }

        private void UpdateIsPaneOpen(bool open)
        {
            if (open)
                IsOpen = true;
            else
                // 折叠窗口
                if (DisplayMode == SplitViewDisplayMode.Overlay && IsOpen)
                IsOpen = false;
            else if (DisplayMode == SplitViewDisplayMode.CompactOverlay && IsOpen)
                IsOpen = false;
            else if (DisplayMode == SplitViewDisplayMode.CompactInline && IsOpen)
                IsOpen = false;
        }
        private void UpdateSecondaryButtonOrientation()
        {
            if (_SecondaryButtonStackPanel == null)
                return;
            if (SecondaryButtonOrientation.Equals(Orientation.Horizontal) && IsOpen)
                _SecondaryButtonStackPanel.Orientation = Orientation.Horizontal;
            else
                _SecondaryButtonStackPanel.Orientation = Orientation.Vertical;
        }
        private void UpdateHamburgerButtonGridWidthToFillAnyGap()
        {
            if (DisplayMode == SplitViewDisplayMode.Inline || DisplayMode == SplitViewDisplayMode.CompactInline)
                HamburgerButtonGridWidth = (IsOpen) ? ShellSplitView.OpenPaneLength : 48;
            else
                HamburgerButtonGridWidth = 48;
        }
        private async Task UpdateSelectedAsync(HamburgerButtonInfo previous, HamburgerButtonInfo value)
        {
            DebugWrite($"OldValue: {previous}, NewValue: {value}");

            if (previous != null)
            {
                IsOpen = (DisplayMode == SplitViewDisplayMode.CompactInline && IsOpen);
            }

            // signal previous
            if (previous != null && previous != value && previous.IsChecked.Value)
            {
                previous.IsChecked = false;
                previous.RaiseUnselected();

                // Workaround for visual state of ToggleButton not reset correctly
                if (value != null)
                {
                    var control = LoadedNavButtons.First(x => x.HamburgerButtonInfo == value).GetElement<Control>();
                    VisualStateManager.GoToState(control, "Normal", true);
                }
            }

            // navigate only when all navigation buttons have been loaded
            if (AllNavButtonsAreLoaded && value?.PageType != null)
            {
                //if (await NavigationService.NavigateAsync(value.PageType, value?.PageParameter, value?.NavigationTransitionInfo))
                //{
                //    IsOpen = (DisplayMode == SplitViewDisplayMode.CompactInline && IsOpen);
                //    if (value.ClearHistory)
                //        NavigationService.ClearHistory();
                //    if (value.ClearCache)
                //        NavigationService.ClearCache(true);
                //}
                //else if (NavigationService.CurrentPageType == value.PageType
                //     && (NavigationService.CurrentPageParam ?? string.Empty) == (value.PageParameter ?? string.Empty))
                //{
                //    if (value.ClearHistory)
                //        NavigationService.ClearHistory();
                //    if (value.ClearCache)
                //        NavigationService.ClearCache(true);
                //}
                //else if (NavigationService.CurrentPageType == value.PageType)
                //{
                //    // just check it
                //}
                //else
                //{
                //    return;
                //}
            }

            // that's it if null
            if (value == null)
            {
                return;
            }
            else
            {
                value.IsChecked = (value.ButtonType == HamburgerButtonInfo.ButtonTypes.Toggle);
                if (previous != value)
                {
                    value.RaiseSelected();
                }
            }
        }

        private void UpdateControl(bool? manualFullScreen = null)
        {
            DebugWrite($"Manual: {manualFullScreen}, IsFullScreen: {IsFullScreen} DisplayMode: {DisplayMode}");

            UpdateFullScreen(manualFullScreen);
        }

        private void UpdateFullScreen(bool? manual = null)
        {
            var opacity = 1;
            if (manual ?? IsFullScreen)
            {
                opacity = 0;
                if (DisplayMode == SplitViewDisplayMode.Overlay)
                    Margin = new Thickness(0, 0, 0, 0);
                else if (IsOpen)
                    Margin = new Thickness(-PaneWidth, 0, 0, 0);
                else
                    Margin = new Thickness(-48, 0, 0, 0);
            }
            else
                Margin = new Thickness(0);

            // 隐藏控件避免闪烁
            Header.Opacity = opacity;
            HamburgerButton.Opacity = opacity;
            HamburgerBackground.Opacity = opacity;
            PaneContent.Opacity = opacity;
        }

        private void UpdateVisualStates()
        {
            var mode = DisplayMode;
            var state = VisualStateGroup.CurrentState ?? VisualStateNormal;
            if (state == VisualStateNarrow)
            {
                mode = SplitViewDisplayMode.Overlay;
            }
            else if (state == VisualStateNormal)
            {
                mode = SplitViewDisplayMode.CompactOverlay;
            }
            else if (state == VisualStateWide)
            {
                mode = SplitViewDisplayMode.CompactInline;
            }
            if (DisplayMode != mode)
            {
                DisplayMode = mode;
            }
            switch (mode)
            {
                case SplitViewDisplayMode.CompactInline:
                    IsOpen = true;
                    break;
                default:
                    IsOpen = false;
                    break;
            }
        }
        public void UpdatePaneMarginToShowHamburgerButton()
        {
            if (HamburgerButtonVisibility == Visibility.Collapsed && HeaderContent == null)
                PaneContent.Margin = new Thickness(0, 0, 0, 0);
            else
                PaneContent.Margin = new Thickness(0, 48, 0, 0);
        }

        #endregion

        private StackPanel _SecondaryButtonStackPanel;
        private void SecondaryButtonStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _SecondaryButtonStackPanel = sender as StackPanel;
            UpdateSecondaryButtonOrientation();
        }
        
        #region Nav Buttons

        #region Commands

        void ExecuteHamburger()
        {
            DebugWrite();

            IsOpen = !IsOpen;
        }
        void ExecuteNav(HamburgerButtonInfo commandInfo)
        {
            DebugWrite($"HamburgerButtonInfo: {commandInfo}");

            if (commandInfo == null)
            {
                throw new NullReferenceException("CommandParameter is not set");
            }

            if (commandInfo.PageType != null)
            {
                Selected = commandInfo;
            }
            else
            {
                ExecuteNavButtonICommand(commandInfo);
                commandInfo.RaiseTapped(new RoutedEventArgs());
            }
        }
        private void ExecuteNavButtonICommand(HamburgerButtonInfo info)
        {
            ICommand command = info.Command;
            if (command != null)
            {
                var commandParameter = info.CommandParameter;
                if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
        }

        #endregion

        int NavButtonCount => PrimaryButtons.Count + SecondaryButtons.Count;
        bool AllNavButtonsAreLoaded => LoadedNavButtons.Count >= NavButtonCount;

        public object PropertyChangedHandlers { get; private set; }

        readonly List<InfoElement> LoadedNavButtons = new List<InfoElement>();

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

        private void NavButton_Loaded(object sender, RoutedEventArgs e)
        {
            DebugWrite();

            var button = new InfoElement(sender);
            if (!LoadedNavButtons.Any(x => x.FrameworkElement == button.FrameworkElement))
            {
                LoadedNavButtons.Add(button);
                if (AllNavButtonsAreLoaded)
                {
                    //HighlightCorrectButton(NavigationService.CurrentPageType, NavigationService.CurrentPageParam);
                }
            }
        }
        private void NavButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            DebugWrite();

            var button = new InfoElement(sender);
            button.HamburgerButtonInfo.RaiseRightTapped(e);
            e.Handled = true;
        }
        private void NavButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            DebugWrite();

            var button = new InfoElement(sender);
            button.HamburgerButtonInfo.RaiseHolding(e);
            e.Handled = true;
        }
        private void NavButtonChecked(object sender, RoutedEventArgs e)
        {
            DebugWrite();

            var button = new InfoElement(sender);
            if (button.HamburgerButtonInfo.ButtonType == HamburgerButtonInfo.ButtonTypes.Toggle)
            {
                button.HamburgerButtonInfo.IsChecked = (button.HamburgerButtonInfo.ButtonType == HamburgerButtonInfo.ButtonTypes.Toggle);
                if (button.HamburgerButtonInfo.IsChecked ?? true) Selected = button.HamburgerButtonInfo;
                if (button.HamburgerButtonInfo.IsChecked ?? true) button.HamburgerButtonInfo.RaiseChecked(e);
                button.FrameworkElement.IsHitTestVisible = !button.HamburgerButtonInfo.IsChecked ?? false;
            }
        }
        private void NavButtonUnchecked(object sender, RoutedEventArgs e)
        {
            DebugWrite();

            var button = new InfoElement(sender);
            if (button.HamburgerButtonInfo.ButtonType == HamburgerButtonInfo.ButtonTypes.Toggle)
            {
                button.HamburgerButtonInfo.RaiseUnchecked(e);
                button.FrameworkElement.IsHitTestVisible = true;
                VisualStateManager.GoToState(button.ToggleButton, "Normal", true);
            }
        }
        private void NavButton_Tapped(object sender, TappedRoutedEventArgs e) => e.Handled = true;

        #endregion

        #region  Touch gesture to OpenClose

        private void PaneContent_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
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
        private void PaneContent_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
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
    }
}
