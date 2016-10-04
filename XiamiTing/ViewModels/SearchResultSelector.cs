using JacobC.Xiami.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JacobC.Xiami.ViewModels
{
    public class SearchResultSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item == Separator)
                return SeparatorTemplate;
            if (item is SongModel)
                return SongTemplate;
            if (item is AlbumModel)
                return AlbumTemplate;
            if (item is ArtistModel)
                return ArtistTemplate;
            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        public DataTemplate SongTemplate { get; set; }
        public DataTemplate AlbumTemplate { get; set; }
        public DataTemplate ArtistTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }

        public readonly static XiamiModelBase Separator = new XiamiModelBase();
    }
}
