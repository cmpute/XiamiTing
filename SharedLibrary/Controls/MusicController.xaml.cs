using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
    /// <summary>
    /// 音乐播放控制器的自定义控件
    /// </summary>
    public sealed partial class MusicController : UserControl
    {
        public static MusicController Instance;

        public MusicController()
        {
            this.InitializeComponent();
            Instance = this;
            this.AddListeners();
        }

        //TODO: 区分是Timer改变的ProgressBar还是进度条自行变化，自行变化时需要改变时间显示。可以考虑用VisualTree获取子控件然后用Tag属性作为是否按下的传递
        private void ProgressBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //Debug.WriteLine(e.NewValue - e.OldValue);
            if (timerblocked)
                return;
            PlaybackService.Instance.CurrentPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
        }
    }   
}
