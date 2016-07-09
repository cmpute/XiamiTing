using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace JacobC.Controls.Behavior
{
    public class MessageDialogAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter) => ExecuteAsync(sender, parameter);

        Boolean busy = false;
        public async Task ExecuteAsync(object sender, object parameter)
        {
            if (busy)
            {
                return;
            }
            busy = true;
            try
            {
                var d = new ContentDialog { Title = Title, Content = Content, PrimaryButtonText = OkText };
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await d.ShowAsync());
            }
            finally
            {
                busy = false;
            }
        }

        /// <summary>
        /// 消息框内容
        /// </summary>
        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(string),
                typeof(MessageDialogAction), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 消息框标题
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string),
                typeof(MessageDialogAction), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 确认文字
        /// </summary>
        public string OkText
        {
            get { return (string)GetValue(OkTextProperty); }
            set { SetValue(OkTextProperty, value); }
        }
        public static readonly DependencyProperty OkTextProperty =
            DependencyProperty.Register(nameof(OkText), typeof(string),
                typeof(MessageDialogAction), new PropertyMetadata("Ok"));

    }

}
