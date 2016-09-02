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
    public class ArtistViewModel : ViewModelBase
    {
        public ArtistViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //DesignData
            }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            uint id = 0;
            if (!uint.TryParse(parameter.ToString(), out id))
                NavigationService.Navigate(typeof(Views.LibraryPage));
            Artist = ArtistModel.GetNew(id);
            if (Artist.Profile == null)
                await Net.WebApi.Instance.GetArtistInfo(Artist);
        }

        ArtistModel _Artist = default(ArtistModel);
        /// <summary>
        /// 获取或设置Artist属性
        /// </summary>
        public ArtistModel Artist { get { return _Artist; } set { Set(ref _Artist, value); } }

    }
}
