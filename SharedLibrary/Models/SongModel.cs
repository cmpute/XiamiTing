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

namespace JacobC.Xiami.Models
{
    /// <summary>
    /// 歌曲的MVVM的Model兼ViewModel模型
    /// </summary>
    [DataContract]
    public class SongModel : BindableBase
    {
        #region Binding Needed

        string _Title = default(string);
        /// <summary>
        /// 获取或设置歌曲的标题
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get { return _Title; } set { Set(ref _Title, value); } }

        ArtistModel _Artist = default(ArtistModel);
        /// <summary>
        /// 获取或设置音轨艺术家
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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

        AlbumModel _Album = default(AlbumModel);
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

        bool _IsLoved = default(bool);
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

        #region Visual Binding Needed Only

        /// <summary>
        /// 获取或设置在列表中的位置，非必须成员
        /// </summary>
        public int ListIndex { get; set; }

        bool _IsSelected = false;
        /// <summary>
        /// 获取或设置是否选中
        /// </summary>
        public bool IsSelected { get { return _IsSelected; } set { Set(ref _IsSelected, value); } }

        bool _IsSelectedOrHovered = default(bool);
        /// <summary>
        /// 获取或设置是否悬浮
        /// </summary>
        public bool IsHovered { get { return _IsSelectedOrHovered; } set { Set(ref _IsSelectedOrHovered, value); } }

        bool _IsPlaying = default(bool);
        /// <summary>
        /// 获取或设置是否正在播放
        /// </summary>
        public bool IsPlaying { get { return _IsPlaying; } set { Set(ref _IsPlaying, value); } }


        private DelegateCommand<object> _DeleteCommand;
        public DelegateCommand<object> DeleteCommand => _DeleteCommand ?? (_DeleteCommand = new DelegateCommand<object>((model) =>
        {
            //TODO: 判断是否在播放
            Services.PlaylistService.Instance.Playlist.Remove(this);
        }));

        private DelegateCommand<object> _LoveCommand;
        public DelegateCommand<object> LoveCommand => _LoveCommand ?? (_LoveCommand = new DelegateCommand<object>((model) =>
        {
            IsLoved = !IsLoved;
        }));

        #endregion

        /// <summary>
        /// 获取或设置音乐文件的链接
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Uri MediaUri { get; set; }

        public int XiamiID { get; set; }

    }
}
