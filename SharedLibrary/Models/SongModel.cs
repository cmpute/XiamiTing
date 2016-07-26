using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;
using Template10.Common;
using JacobC.Xiami.Services;
using System.Runtime.CompilerServices;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 歌曲的MVVM的Model兼ViewModel模型
    /// </summary>
    [DataContract]
    public class SongModel : LovableModelBase
    {

        static Dictionary<uint, SongModel> _dict;
        static SongModel()
        {
            if (SettingsService.Instance.CacheItemsInDict)
                _dict = new Dictionary<uint, SongModel>();
        }
        /// <summary>
        /// 获取一个新的<see cref="SongModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="xiamiID">标志<see cref="SongModel"/>的虾米ID</param>
        /// <returns></returns>
        public static SongModel GetNew(uint xiamiID)
        {
            SongModel song = null;
            if (!(_dict?.TryGetValue(xiamiID, out song) ?? false))
            {
                song = new SongModel() { XiamiID = xiamiID };
                _dict?.Add(xiamiID, song);
            }
            return song;
        }
        private SongModel() { }

        #region Binding Needed

        //ArtistModel _Artist = null;
        ///// <summary>
        ///// 获取或设置专辑艺术家
        ///// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public ArtistModel Artist
        //{
        //    get { return _Artist; }
        //    set
        //    {
        //        if (!object.Equals(_Artist, value))
        //        {
        //            if (_Artist != null) _Artist.PropertyChanged -= _Artist_PropertyChanged;
        //            _Artist = value;
        //            if (_Artist != null) _Artist.PropertyChanged += _Artist_PropertyChanged;
        //        }
        //    }
        //}
        //private void _Artist_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => RaisePropertyChanged(nameof(Artist));

        AlbumModel _Album = null;
        /// <summary>
        /// 获取或设置音轨所属的的专辑
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AlbumModel Album
        {
            get { return _Album; }
            set
            {
                if (!object.Equals(_Album, value))
                {
                    if (_Album != null) _Album.PropertyChanged -= _Album_PropertyChanged;
                    _Album = value;
                    if (_Album != null) _Album.PropertyChanged += _Album_PropertyChanged;
                    this.RaisePropertyChanged();
                }
            }
        }
        private void _Album_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => RaisePropertyChanged(nameof(Album));

        #endregion

        /// <summary>
        /// 获取或设置音乐文件的链接
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri MediaUri { get; set; }

        int _PlayCount = -1;
        /// <summary>
        /// 获取或设置播放次数属性
        /// </summary>
        public int PlayCount { get { return _PlayCount; } set { Set(ref _PlayCount, value); } }

        int _ShareCount = -1;
        /// <summary>
        /// 获取或设置分享次数属性
        /// </summary>
        public int ShareCount { get { return _ShareCount; } set { Set(ref _ShareCount, value); } }

        string _TrackArtist = null;
        /// <summary>
        /// 获取或设置音轨艺术家属性
        /// </summary>
        [JsonProperty]
        public string TrackArtist
        {
            get { return _TrackArtist ?? Album?.Artist?.Name; }
            set { Set(ref _TrackArtist, value); }
        }

        string _Composer = null;
        /// <summary>
        /// 获取或设置作曲家属性
        /// </summary>
        public string Composer { get { return _Composer; } set { Set(ref _Composer, value); } }

        string _Lyricist = null;
        /// <summary>
        /// 获取或设置作词家属性
        /// </summary>
        public string Lyricist { get { return _Lyricist; } set { Set(ref _Lyricist, value); } }

        string _Arranger = default(string);
        /// <summary>
        /// 获取或设置编曲家属性
        /// </summary>
        public string Arranger { get { return _Arranger; } set { Set(ref _Arranger, value); } }

        IEnumerable<CollectionModel> _RelateHotCollections = default(IEnumerable<CollectionModel>);
        /// <summary>
        /// 获取或设置推荐的精选集属性
        /// </summary>
        public IEnumerable<CollectionModel> RelateHotCollections { get { return _RelateHotCollections; } set { Set(ref _RelateHotCollections, value); } }


        public override string ToString()
        {
            return $@"标题：{Name}  ID：{XiamiID} 描述：{Description}
播放：{PlayCount}  分享：{ShareCount}
专辑艺人：{Album?.Artist?.Name}  音轨艺人：{TrackArtist}
作词：{Lyricist}  作曲：{Composer}  编曲：{Arranger}
";
        }
    }
}
