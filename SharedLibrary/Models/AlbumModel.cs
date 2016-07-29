using JacobC.Xiami.Services;
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
    public class AlbumModel : LovableModelBase<uint>
    {
        static Dictionary<uint, AlbumModel> _dict;
        static AlbumModel()
        {
            if (SettingsService.Instance.CacheItemsInDict)
                _dict = new Dictionary<uint, AlbumModel>();
        }
        /// <summary>
        /// 获取一个新的<see cref="AlbumModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="xiamiID">标志<see cref="AlbumModel"/>的虾米ID</param>
        /// <returns></returns>
        public static AlbumModel GetNew(uint xiamiID)
        {
            AlbumModel album = null;
            if (!(_dict?.TryGetValue(xiamiID, out album) ?? false))
            {
                album = new AlbumModel() { XiamiID = xiamiID };
                _dict?.Add(xiamiID, album);
            }
            return album;
        }
        private AlbumModel() { }

        /* 专辑封面后缀说明
         * ..:原图
         * .._1:100x100
         * .._2:185x184
         * .._3:55x55
         * .._4:原图
         * .._5:185x185
         */
        [JsonProperty]
        Uri _AlbumArtUri = new Uri(@"ms-appx:///Assets/Pictures/cd100.gif");
        /// <summary>
        /// 获取或设置专辑封面的链接
        /// </summary>
        public Uri AlbumArtUri
        {
            get { return _AlbumArtUri; }
            set
            {
                if (_AlbumArtUri?.ToString() != value?.ToString())
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
                if (_AlbumArtFullUri?.ToString() != value?.ToString())
                    Set(ref _AlbumArtFullUri, value);
            }
        }

        ArtistModel _Artist = null;
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

        string _Rating = null;
        /// <summary>
        /// 获取或设置专辑的评分
        /// </summary>
        public string Rating { get { return _Rating; } set { Set(ref _Rating, value); } }

        int[] _RatingDetail = new int[5] { -1, -1, -1, -1, -1 };
        /// <summary>
        /// 获取或设置专辑的详细评分
        /// </summary>
        public int[] RatingDetail
        {
            get { return _RatingDetail; }
            set
            {
                if (value.Length != 5) throw new InvalidOperationException("评分应为五个档");
                Set(ref _RatingDetail, value);
            }
        }

        string _Language = default(string);
        /// <summary>
        /// 获取或设置专辑的语种
        /// </summary>
        public string Language { get { return _Language; } set { Set(ref _Language, value); } }

        string _Publisher = default(string);
        /// <summary>
        /// 获取或设置专辑的发行公司
        /// </summary>
        public string Publisher { get { return _Publisher; } set { Set(ref _Publisher, value); } }

        string _Type = default(string);
        /// <summary>
        /// 获取或设置专辑的类别
        /// </summary>
        public string Type { get { return _Type; } set { Set(ref _Type, value); } }

        string _Introduction = default(string);
        /// <summary>
        /// 获取或设置专辑介绍
        /// </summary>
        public string Introduction { get { return _Introduction; } set { Set(ref _Introduction, value); } }


        IList<SongModel> _SongList = null;
        /// <summary>
        /// 获取或设置专辑所含歌曲属性
        /// </summary>
        public IList<SongModel> SongList { get { return _SongList; } set { Set(ref _SongList, value); } }

        IList<AlbumModel> _RelateHotAlbums = null;
        /// <summary>
        /// 获取或设置该艺人其他热门专辑属性
        /// </summary>
        public IList<AlbumModel> RelateHotAlbums { get { return _RelateHotAlbums; } set { Set(ref _RelateHotAlbums, value); } }

        IList<GenreModel> _Genre = default(IList<GenreModel>);
        /// <summary>
        /// 获取或设置专辑的风格
        /// </summary>
        public IList<GenreModel> Genre { get { return _Genre; } set { Set(ref _Genre, value); } }


        string _ReleaseDate = null; //使用DateTime的话会引入DateTimeFormatInfo增加内存消耗
        /// <summary>
        /// 获取或设置发售日期属性
        /// </summary>
        public string ReleaseDate { get { return _ReleaseDate; } set { Set(ref _ReleaseDate, value); } }

        public override string ToString()
        {
            return $@"名称：{Name}  ID:{XiamiID}
评分：{Rating}  发售日期：{ReleaseDate}";
        }
    }
}
