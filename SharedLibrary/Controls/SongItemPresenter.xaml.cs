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
using JacobC.Xiami.Services;

namespace JacobC.Xiami.Controls
{
    public sealed partial class SongItemPresenter : ContentControl
    {
        public SongItemPresenter()
        {
            this.InitializeComponent();
        }

        VisualStateGroup _LinkedGroup = null;

        protected override void OnApplyTemplate()
        {
            //System.Diagnostics.Debugger.Break();
            base.OnApplyTemplate();
            bool found = false;
            DependencyObject obj = this;
            while (!found)
            {
                obj = VisualTreeHelper.GetParent(obj);
                if (obj is Grid)
                    if ((obj as Grid).Name == "ContentBorder")
                        found = true;
            }
            if (found)
            {
                var groups = VisualStateManager.GetVisualStateGroups(obj as FrameworkElement);
                foreach (var group in groups)
                {
                    if (group.Name == "CommonStates")
                    {
                        group.CurrentStateChanged += Group_CurrentStateChanged;
                        _LinkedGroup = group;
                    }
                }
            }
        }



        public static EventHandler GetMyProperty(DependencyObject obj)
        {
            return (EventHandler)obj.GetValue(MyPropertyProperty);
        }

        public static void SetMyProperty(DependencyObject obj, EventHandler value)
        {
            obj.SetValue(MyPropertyProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.RegisterAttached("MyProperty", typeof(EventHandler), typeof(SongItemPresenter), null);



        protected override void OnDisconnectVisualChildren()
        {
            _LinkedGroup.CurrentStateChanged -= Group_CurrentStateChanged;
            _LinkedGroup = null;
        }

        private void Group_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            LogService.DebugWrite($"VisualState:{e.NewState.Name}", nameof(SongItemPresenter));
            InternalVisualStateChanged(e);
            VisualStateChanged?.Invoke(this, e); 
        }

        /// <summary>
        /// 当所属的<see cref="ListViewItem"/>或<see cref="GridViewItem"/>的VisualState发生变化时发生
        /// </summary>
        public event VisualStateChangedEventHandler VisualStateChanged;
        private void InternalVisualStateChanged(VisualStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(ElementVisualStateManager.GetVisualStateGroups(this.Content as FrameworkElement)?.Count);
            System.Diagnostics.Debug.WriteLine(
            ElementVisualStateManager.GoToElementState((this.Content as FrameworkElement), e.NewState.Name, true));
        }
    }

    public class ElementVisualStateManager : VisualStateManager
    {
        protected override bool GoToStateCore(Control control, FrameworkElement stateGroupsRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
        {
            if ((group == null) || (state == null))
            {
                return false;
            }

            if (control == null)
            {
                control = new ContentControl();
            }

            return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
        }

        public static bool GoToElementState(FrameworkElement stateGroupsRoot, string stateName, bool useTransitions)
        {
            var root = FindNearestStatefulFrameworkElement(stateGroupsRoot);

            var customVisualStateManager = GetCustomVisualStateManager(root) as ElementVisualStateManager;

            return ((customVisualStateManager != null) && customVisualStateManager.GoToStateInternal(root, stateName, useTransitions));
        }

        private static FrameworkElement FindNearestStatefulFrameworkElement(FrameworkElement element)
        {
            while (element != null && VisualStateManager.GetCustomVisualStateManager(element) == null)
            {
                element = element.Parent as FrameworkElement;
            }

            return element;
        }

        private bool GoToStateInternal(FrameworkElement stateGroupsRoot, string stateName, bool useTransitions)
        {
            VisualStateGroup group;
            VisualState state;

            return (TryGetState(stateGroupsRoot, stateName, out group, out state) && this.GoToStateCore(null, stateGroupsRoot, stateName, group, state, useTransitions));
        }

        

        private static bool TryGetState(FrameworkElement element, string stateName, out VisualStateGroup group, out VisualState state)
        {

            group = null;
            state = null;

            foreach (VisualStateGroup group2 in VisualStateManager.GetVisualStateGroups(element))
            {
                foreach (VisualState state2 in group2.States)
                {
                    if (state2.Name == stateName)
                    {
                        group = group2;
                        state = state2;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
