using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System.ComponentModel;

namespace JacobC.Xiami.ViewModels
{
    public class SongViewModel : BindableBase
    {
        SongModel _Model;
        /// <summary>
        /// 歌曲ViewModel的Model源
        /// </summary>
        public SongModel Model {
            get { return _Model; }
            set
            {
                if (!object.Equals(_Model, value))
                {
                    if (_Model != null) _Model.PropertyChanged -= _Model_PropertyChanged;
                    _Model = value;
                    if (_Model != null) _Model.PropertyChanged += _Model_PropertyChanged;
                    this.RaisePropertyChanged();
                }
            }
        }
        private void _Model_PropertyChanged(object sender, PropertyChangedEventArgs e) => RaisePropertyChanged(nameof(Model));

        public SongViewModel(SongModel source)
        {
            Model = source;
        }

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
            Model.IsLoved = !Model.IsLoved;
        }));

        private DelegateCommand<object> _PlayTrackCommand;
        public DelegateCommand<object> PlayTrackCommand => _PlayTrackCommand ?? (_PlayTrackCommand = new DelegateCommand<object>((model) =>
        {
            PlaybackService.Instance.PlayTrack(this.Model);
            PlaylistService.Instance.CurrentPlaying = this;
        }));
    }
}
