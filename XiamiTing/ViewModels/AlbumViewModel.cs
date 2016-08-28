using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Navigation;

namespace JacobC.Xiami.ViewModels
{
    public class AlbumViewModel : ViewModelBase
    {
        public AlbumViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //DesignData
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            uint id;
            if (!uint.TryParse(parameter.ToString(), out id))
                NavigationService.Navigate(typeof(Views.LibraryPage));
            Album = AlbumModel.GetNew(id);
            if (Album.Description == null)
                await Net.WebApi.Instance.GetAlbumInfo(Album);
        }

        AlbumModel _Album = default(AlbumModel);
        /// <summary>
        /// 获取或设置内容专辑属性
        /// </summary>
        public AlbumModel Album { get { return _Album; } set { Set(ref _Album, value); } }
    }
}
