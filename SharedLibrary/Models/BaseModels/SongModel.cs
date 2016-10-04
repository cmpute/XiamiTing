using JacobC.Xiami.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 歌曲的MVVM的Model兼ViewModel模型
    /// </summary>
    [DataContract]
    public class SongModel : LovableModelBase<uint>
    {

        //TODO: 在Artist,Album,Song之间使用弱引用WeakReference
        static Dictionary<uint, SongModel> _dict = new Dictionary<uint, SongModel>();
        public static readonly SongModel Null = new SongModel() { Name = "虾米音乐", TrackArtist = "随心而动", Album = AlbumModel.Null };
        /// <summary>
        /// 获取一个新的<see cref="SongModel"/>实例，如果已经创建过则返回这个实例
        /// </summary>
        /// <param name="xiamiID">标志<see cref="SongModel"/>的虾米ID</param>
        /// <returns></returns>
        public static SongModel GetNew(uint xiamiID)
        {
            if (xiamiID == 0)
                throw new ArgumentException("歌曲ID错误");
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

        MVModel _MV = default(MVModel);
        /// <summary>
        /// 获取或设置MV(地址)
        /// </summary>
        public MVModel MV { get { return _MV; } set { Set(ref _MV, value); } }

        #endregion

        /// <summary>
        /// 获取或设置音乐文件的链接
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri MediaUri { get; set; }
        /// <summary>
        /// 获取或设置音乐的长度（播放时直接从后台获取长度即可）
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        bool _Available = default(bool);
        /// <summary>
        /// 获取或设置歌曲是否可用（而非下架或者被删除）
        /// </summary>
        public bool Available { get { return _Available; } set { Set(ref _Available, value); } }

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

        int _TrackID = default(int);
        /// <summary>
        /// 获取或设置音轨号
        /// </summary>
        public int TrackID { get { return _TrackID; } set { Set(ref _TrackID, value); } }

        string _DiscID = default(string);
        /// <summary>
        /// 获取或设置专辑的Disc序号或Disc说明
        /// </summary>
        public string DiscID { get { return _DiscID; } set { Set(ref _DiscID, value); } }

        TimeSpan _AudioLength = default(TimeSpan);
        /// <summary>
        /// 获取或设置音频时长
        /// </summary>
        public TimeSpan AudioLength { get { return _AudioLength; } set { Set(ref _AudioLength, value); } }

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

        IList<CollectionModel> _RelateHotCollections = default(IList<CollectionModel>);
        /// <summary>
        /// 获取或设置推荐的精选集属性
        /// </summary>
        public IList<CollectionModel> RelateHotCollections { get { return _RelateHotCollections; } set { Set(ref _RelateHotCollections, value); } }

        IList<SongModel> _RelateHotSongs = default(IList<SongModel>);
        /// <summary>
        /// 获取或设置相似歌曲推荐
        /// </summary>
        public IList<SongModel> RelateHotSongs { get { return _RelateHotSongs; } set { Set(ref _RelateHotSongs, value); } }

        /// <summary>
        /// 获取和设置歌曲与某一首歌是同一首
        /// </summary>
        public SongModel DuplicateOf { get; set; }

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
