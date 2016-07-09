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

        #endregion

        private StackPanel _SecondaryButtonStackPanel;
    }
}
