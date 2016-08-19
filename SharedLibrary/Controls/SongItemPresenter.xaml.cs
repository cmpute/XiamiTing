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

namespace JacobC.Xiami.Controls
{
    public sealed partial class SongItemPresenter : ContentControl
    {
        public SongItemPresenter()
        {
            this.InitializeComponent();
        }

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
                        group.CurrentStateChanged += Group_CurrentStateChanged;
                }
            }
        }

        private void Group_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.NewState.Name);
            InternalVisualStateChanged(e);
            VisualStateChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 当所属的<see cref="ListViewItem"/>或<see cref="GridViewItem"/>的VisualState发生变化时发生
        /// </summary>
        public event VisualStateChangedEventHandler VisualStateChanged;
        private void InternalVisualStateChanged(VisualStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(this.DataContext.GetType().ToString());
        }
    }
}
