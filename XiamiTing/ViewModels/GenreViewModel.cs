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
    public class GenreViewModel : ViewModelBase
    {
        public GenreViewModel()
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
            Genre = GenreModel.GetNew(id);
            //if (Genre.Introduction == null)
            //    await Net.WebApi.Instance.GetAlbumInfo(Album);
        }

        GenreModel _Genre = default(GenreModel);
        /// <summary>
        /// 获取或设置Genre属性
        /// </summary>
        public GenreModel Genre { get { return _Genre; } set { Set(ref _Genre, value); } }
        
    }
}
