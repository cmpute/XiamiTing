using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace JacobC.Xiami.Models
{
    [DataContract]
    public class AlbumModel : BindableBase
    {
        Uri _AlbumArtUri = new Uri(@"ms-appx:///Assets/Pictures/cd100.gif");
        /// <summary>
        /// 获取或设置专辑封面的链接
        /// </summary>
        public Uri AlbumArtUri
        {
            get { return _AlbumArtUri; }
            set
            {
                GetAlbumArtCache();
                Set(ref _AlbumArtUri, value);
            }
        }

        /// <summary>
        /// 获取专辑封面的本地缓存
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri AlbumArtCacheUri { get; private set; } = new Uri(@"ms-appx:///Assets/Pictures/cd100.gif");

        protected async void GetAlbumArtCache()
        {
            await Task.Run(() =>
            {
                //TODO: 从AlbumArtUri获取专辑封面缓存
                AlbumArtCacheUri = new Uri(@"ms-appx:///Assets/Pictures/cd100.gif");
            });
        }

        string _Name = default(string);
        /// <summary>
        /// 获取或设置专辑的名称
        /// </summary>
        public string Name { get { return _Name; } set { Set(ref _Name, value); } }

        ArtistModel _Artist = default(ArtistModel);
        /// <summary>
        /// 获取或设置专辑艺术家
        /// </summary>
        public ArtistModel Artist
        {
            get { return _Artist; }
            set
            {
                if (!object.Equals(_Artist, value))
                {
                    if (_Artist != null) _Artist.PropertyChanged -= _Artist_PropertyChanged;
                    _Artist = value;
                    if (_Artist != null) _Artist.PropertyChanged += _Artist_PropertyChanged;
                    this.RaisePropertyChanged();
                }
            }
        }
        private void _Artist_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => RaisePropertyChanged(nameof(Artist));

        /// <summary>
        /// 获取或设置专辑的虾米ID
        /// </summary>
        public int AlbumID { get; set; }
    }
}
