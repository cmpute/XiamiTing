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
    public class AlbumModel : LovableModelBase<uint>, ICovered
    {
        static Dictionary<uint, AlbumModel> _dict = new Dictionary<uint, AlbumModel>();
        public static readonly AlbumModel Null = new AlbumModel() { };
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

        public const string SmallDefaultUri = @"ms-appx:///Assets/Pictures/cd100.gif";
        public const string LargeDefaultUri = @"ms-appx:///Assets/Pictures/cd500.gif";

        #region ICovered Members
        [JsonProperty]
        Uri _AlbumArtUri = new Uri(SmallDefaultUri);
        /// <summary>
        /// 获取或设置专辑封面的链接
        /// </summary>
        public Uri Art
        {
            get { return _AlbumArtUri; }
            set
            {
                if (_AlbumArtUri?.ToString() != value?.ToString())
                    Set(ref _AlbumArtUri, value);
            }
        }

        [JsonProperty]
        Uri _AlbumArtFullUri = new Uri(LargeDefaultUri);
        /// <summary>
        /// 获取或设置专辑原图的链接
        /// </summary>
        public Uri ArtFull
        {
            get { return _AlbumArtFullUri; }
            set
            {
                if (_AlbumArtFullUri?.ToString() != value?.ToString())
                    Set(ref _AlbumArtFullUri, value);
            }
        }

        /// <summary>
        /// 获取专辑的大图链接
        /// </summary>
        public Uri ArtLarge => GetArtWithSize(5);

        /// <summary>
        /// 获取指定大小的封面地址
        /// </summary>
        /// <param name="size">封面大小对应的数字
        /// 专辑：0:原图 1:100x100 2:185x184(按比例) 3:55x55 4:原图 5:185x185
        /// </param>
        public Uri GetArtWithSize(int sizecode)
        {
            var origin = ArtFull.ToString();
            return new Uri(origin.Insert(origin.LastIndexOf('.'), "_" + (sizecode == 0 ? "" : sizecode.ToString())));
        }
        #endregion


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

        string _ArtistHtml = null;
        /// <summary>
        /// 获取或设置专辑的艺术家名字的HTML形式
        /// </summary>
        public string ArtistHtml { get { return _ArtistHtml; } set { Set(ref _ArtistHtml, value); } }

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
