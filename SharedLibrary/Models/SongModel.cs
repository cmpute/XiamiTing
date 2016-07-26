﻿using System;
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
    public class SongModel : SafeBindableBase
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
        /// <param name="XiamiID">标志<see cref="SongModel"/>的虾米ID</param>
        /// <returns></returns>
        public static SongModel GetNew(uint XiamiID)
        {
            SongModel album = null;
            if (!(_dict?.TryGetValue(XiamiID, out album) ?? false))
            {
                album = new SongModel() { SongID = XiamiID };
                _dict?.Add(XiamiID, album);
            }
            return album;
        }
        private SongModel() { }

        #region Binding Needed

        string _Title = null;
        /// <summary>
        /// 获取或设置歌曲的标题
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get { return _Title; } set { Set(ref _Title, value); } }

        string _AliasTitle = default(string);
        /// <summary>
        /// 获取或设置曲目名称的说明
        /// </summary>
        public string AliasTitle { get { return _AliasTitle; } set { Set(ref _AliasTitle, value); } }


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

        bool _IsLoved = false;
        /// <summary>
        /// 获取或设置歌曲是否标记为喜爱
        /// </summary>
        public bool IsLoved
        {
            get { return _IsLoved; }
            set
            {
                IsLoveChanging?.Invoke(this, new ChangedEventArgs<bool>(_IsLoved, value));
                Set(ref _IsLoved, value);
            }
        }
        public event EventHandler<ChangedEventArgs<bool>> IsLoveChanging;

        #endregion

        /// <summary>
        /// 获取或设置音乐文件的链接
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri MediaUri { get; set; }

        public uint SongID { get; set; }

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

        string _Vocalist = default(string);
        /// <summary>
        /// 获取或设置演唱者
        /// </summary>
        public string Vocalist { get { return _Vocalist; } set { Set(ref _Vocalist, value); } }


        public override string ToString()
        {
            return $@"标题：{Title}  ID：{SongID} 描述：{AliasTitle}
播放：{PlayCount}  分享：{ShareCount}
专辑艺人：{Album?.Artist?.Name}  音轨艺人：{TrackArtist}
作词：{Lyricist}  作曲：{Composer}  编曲：{Arranger} 歌手：{Vocalist}
";
        }
    }
}
