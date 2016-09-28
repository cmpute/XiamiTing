using JacobC.Xiami.Models;
using JacobC.Xiami.Net;
using JacobC.Xiami.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Controls;
using Template10.Mvvm;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace JacobC.Xiami.ViewModels
{
    public class UserInfoPageViewModel : ViewModelBase
    {
        public UserInfoPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Current = UserModel.Null;
                //DesignData
            }
            LoginHelper.UserChanged += LoginHelper_UserChanged;
            LoginHelper.LogStateChanged += LoginHelper_LogStateChanged;
        }

        private void LoginHelper_LogStateChanged(object sender, Template10.Common.ChangedEventArgs<bool> e)
        {
            VisualStateManager.GoToState(View, e.NewValue ? "LoggedIn" : "NotLoggedIn", true);
        }
        private void LoginHelper_UserChanged(object sender, Template10.Common.ChangedEventArgs<uint> e)
        {
            Current = e.NewValue > 0 ? UserModel.GetNew(e.NewValue) : UserModel.Null;
        }

        public UserInfoPage View { get; set; }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
#if DEBUG
            Current = UserModel.GetNew(6069765u);
#else
            Current = LoginHelper.UserId > 0 ? UserModel.GetNew(LoginHelper.UserId) : UserModel.Null;
            if (!LoginHelper.IsLoggedIn)
                await ShowLoginModal();
#endif
            if (Current.CheckWhetherNeedInfo() && Current != UserModel.Null)
                await WebApi.Instance.GetUserInfo(Current);
        }

        UserModel _Current = UserModel.Null;
        /// <summary>
        /// 获取或设置当前用户属性
        /// </summary>
        public UserModel Current { get { return _Current; } set { Set(ref _Current, value); } }

        private DelegateCommand<object> _LogoutCommand;
        public DelegateCommand<object> LogoutCommand => _LogoutCommand ?? (_LogoutCommand = new DelegateCommand<object>( async (model) =>
        {
            if (LoginHelper.IsLoggedIn)
                await LoginHelper.Logout();
            else
                await ShowLoginModal();

        }));

        async Task ShowLoginModal()
        {
            await Dispatcher.DispatchAsync(()=>
                {
                    var modal = Window.Current.Content as ModalDialog;
                    modal.ModalContent = new LoginDialog();
                    modal.ModalBackground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
                    modal.IsModal = true;
                }
            );
        }
    }
}
