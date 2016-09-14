using JacobC.Xiami.Net;
using JacobC.Xiami.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Template10.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace JacobC.Xiami.Views
{
    public sealed partial class LoginDialog : UserControl
    {
        public LoginDialog()
        {
            this.InitializeComponent();
        }

        int _index = 0;
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var param = (sender as RadioButton).Tag as string;
            if (param != null)
            {
                int cindex = int.Parse(param);
                if (cindex != _index)
                {
                    switch(cindex)
                    {
                        case 0:
                        default:
                            LoginSelector.SelectedIndex = 0;
                            break;
                        case 1:
                        case 2:
                        case 3:
                            LoginSelector.SelectedIndex = 1;
                            break;
                    }
                    _index = cindex;
                }
            }
        }
        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            ExitModal();
        }

        private async void TaobaoLogin_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("DOM加载完成");
            var inject = @"window.addEventListener('message',function(args){
                window.external.notify(args.data);
                });";
            await sender.InvokeScriptAsync("eval", new string[] { inject });
        }
        private async void TaobaoLogin_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var t = System.Net.WebUtility.UrlDecode(e.Value);
            System.Diagnostics.Debug.WriteLine("ScriptNotify:" + t);
            if (t.IndexOf("\"st\"") >= 0)
                await LoggedProcess(await LoginHelper.XiamiLogin(
                    JObject.Parse(t).GetValue("st").Value<string>()));
        }
        private void TaobaoLogin_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            System.Diagnostics.Debugger.Break();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await LoggedProcess(await LoginHelper.XiamiLogin(UserName.Text, Password.Password));
        }

        private void ExitModal()
        {
            var modal = this.Parent as ModalDialog;
            if (modal == null)
                modal = Window.Current.Content as ModalDialog;
            modal.IsModal = false;
            modal.ModalContent = null;
        }
        private async Task LoggedProcess(LoginResult result)
        {
            if (result.Status == LoginStatus.Success)
            {
                LogService.DebugWrite("Login Success", nameof(LoginDialog));
                ExitModal();
                await WebApi.Instance.GetUserInfo(Models.UserModel.GetNew(LoginHelper.UserId));
            }
        }
    }
}
