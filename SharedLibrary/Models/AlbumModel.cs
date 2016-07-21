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
        /* 专辑封面后缀说明
         * ..:原图
         * .._1:100x100
         * .._2:185x184
         * .._3:55x55
         * .._4:原图
         * .._5:185x185
         */

        Uri _AlbumArtUri = new Uri(@"ms-appx:///Assets/Pictures/cd100.gif");
        /// <summary>
        /// 获取或设置专辑封面的链接
        /// </summary>
        public Uri AlbumArtUri
        {
            get { return _AlbumArtUri; }
            set
            {
                //TODO: 处理出Uri的前缀文字
                Set(ref _AlbumArtUri, value);
            }
        }

        Uri _AlbumArtFullUri = new Uri(@"ms-appx:///Assets/Pictures/cd500.gif");
        /// <summary>
        /// 获取或设置专辑大图的链接
        /// </summary>
        public Uri AlbumArtFullUri
        {
            get { return _AlbumArtFullUri; }
            set
            {
                Set(ref _AlbumArtFullUri, value);
            }
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
        public uint AlbumID { get; set; } = 0;
    }
}
