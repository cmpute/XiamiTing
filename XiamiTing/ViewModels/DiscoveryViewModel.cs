using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;
using JacobC.Xiami.Services;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading;
using Windows.Foundation;
using JacobC.Xiami.Models;
using JacobC.Xiami.Net;
using Windows.UI.Xaml;
using JacobC.Xiami.Views;
using Windows.UI.Xaml.Documents;

namespace JacobC.Xiami.ViewModels
{
    public class DiscoveryViewModel : ViewModelBase
    {
        public DiscoveryViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //Value = "Designtime value";
            }
        }

        //string _Value = "Gas";
        //public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //if (suspensionState.Any())
            //{
            //    Value = suspensionState[nameof(Value)]?.ToString();
            //}
            //LogService.DebugWrite("Nav to MainPage, mode = " + mode.ToString());
            if (mode == NavigationMode.Back)
                return;
            if (NewInAllRecs == null)
            {
                DailyRecs = await WebApi.Instance.GetDailyRecs();
                var mainbatch = await WebApi.Instance.GetMainRecs();
                NewInAllRecs = mainbatch.NewInAll;
            }
        }


        DailyRecBatch _DailyRecs = default(DailyRecBatch);
        /// <summary>
        /// 获取或设置猜你喜欢属性
        /// </summary>
        public DailyRecBatch DailyRecs { get { return _DailyRecs; } set { Set(ref _DailyRecs, value); } }

        IList<AlbumModel> _NewInAllRecs = default(IList<AlbumModel>);
        /// <summary>
        /// 获取或设置新碟首发属性
        /// </summary>
        public IList<AlbumModel> NewInAllRecs { get { return _NewInAllRecs; } set { Set(ref _NewInAllRecs, value); } }


        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            //if (suspending)
            //{
            //    suspensionState[nameof(Value)] = Value;
            //}
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(SettingsPage), 2);

        public void NavigateAlbum(AlbumModel album)=>
             NavigationService.Navigate(typeof(AlbumPage), album?.XiamiID);

        #region EventHandlers

        public void LibLink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            if (LoginHelper.UserId != 0)
            {
                if (LoginHelper.IsLoggedIn)
                    PlaybackService.Instance.SetPlaybackSource(new RadioService(RadioModel.XiamiCai));
                else
                    PlaybackService.Instance.SetPlaybackSource(new RadioService(RadioModel.GetFromUser(UserModel.GetNew(LoginHelper.UserId))));
            }
            else
                PlaybackService.Instance.SetPlaybackSource(new RadioService(RadioModel.PublicRec));
        }

        #endregion


    }
}

