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
                //DesignData
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!LoginHelper.IsLoggedIn)
            {
                var modal = Window.Current.Content as ModalDialog;
                modal.ModalContent = new LoginDialog();
                modal.ModalBackground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
                modal.IsModal = true;
            }
            await Task.CompletedTask;
        }
    }
}
