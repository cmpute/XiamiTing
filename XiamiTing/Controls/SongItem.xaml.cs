using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Template10.Utils;
using Windows.UI;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace JacobC.Xiami.Controls
{
    public sealed partial class SongItem : UserControl
    {
        public SongItem()
        {
            this.InitializeComponent();
            this.DoubleTapped += (sender, e) => PlayButton_Click(sender, e);
        }


        PlaylistType _listtype = PlaylistType.LocalPlaylist;

        #region Link to Parent

        VisualStateGroup _LinkedGroup = null;
        DependencyObject _LinkedItem = null;
        ItemsControl _LinkedList = null;
        INotifyCollectionChanged _LinkedCollection = null;
        bool isIncreTtem = false;
        private void Group_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            //LogService.DebugWrite($"VisualState:{e.NewState.Name}", nameof(SongItem));
            VisualStateManager.GoToState(this, e.NewState.Name, true);
        }
        private void SongItem_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debugger.Break();
            bool found = false;
            DependencyObject obj = this;
            while (!found)
            {
                obj = VisualTreeHelper.GetParent(obj);
                if (obj is Grid)
                    if ((obj as Grid).Name == "ContentBorder")
                        found = true;
            }
            if (found)
            {
                var item = VisualTreeHelper.GetParent(obj);
                if (item is ListViewItem)
                {
                    _LinkedItem = item;
                    (item as ListViewItem).Tag = this;//用Tag传递对SongItem的引用
                    _LinkedList = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
                    var listcontainer = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(_LinkedList));
                    if (listcontainer is Playlist)
                    {//获取播放列表类型
                        var list = listcontainer as Playlist;
                        list.ListTypeChanged += SongItem_ListTypeChanged;
                        _listtype = list.ListType;
                        VisualStateManager.GoToState(this, list.ListType.ToString(), true);
                    }
                    if (_LinkedList.ItemsSource is INotifyCollectionChanged)
                    {
                        _LinkedCollection = _LinkedList.ItemsSource as INotifyCollectionChanged;
                        _LinkedCollection.CollectionChanged += _Linkedlist_CollectionChanged;
                        isIncreTtem = _LinkedList.ItemsSource is ISupportIncrementalLoading;
                    }
                    var index = isIncreTtem ? (_LinkedCollection as IList<SongModel>).IndexOf(ItemSource) : _LinkedList.IndexFromContainer(item);
                    ListIndex = index + 1;
                    if (PlaylistService.Instance.CurrentIndex == index && index != -1)//正在播放则更新状态
                        VisualStateManager.GoToState(this, "Playing", true);
                    else
                        VisualStateManager.GoToState(this, "NotPlaying", true);
                }

                var groups = VisualStateManager.GetVisualStateGroups(obj as FrameworkElement);
                foreach (var group in groups)
                {
                    if (group.Name == "CommonStates")
                    {
                        group.CurrentStateChanged += Group_CurrentStateChanged;
                        _LinkedGroup = group;
                    }
                }
            }
        }
        private void SongItem_ListTypeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _listtype = (PlaylistType)(e.NewValue);
            VisualStateManager.GoToState(this, e.NewValue.ToString(), true);
        }
        private void SongItem_Unloaded(object sender, RoutedEventArgs e)
        {
            //卸载事件，删除引用
            if (_LinkedGroup != null)
            {
                _LinkedGroup.CurrentStateChanged -= Group_CurrentStateChanged;
                _LinkedGroup = null;
            }
            if (_LinkedCollection != null)
            {
                _LinkedCollection.CollectionChanged -= _Linkedlist_CollectionChanged;
                _LinkedCollection = null;
            }
            if(_LinkedList != null)
            {
                var listcontainer = VisualTreeHelper.GetParent(_LinkedList);
                if (listcontainer is Playlist)
                    (listcontainer as Playlist).ListTypeChanged -= SongItem_ListTypeChanged;
                _LinkedList = null;
            }
            _LinkedItem = null;
        }
        private async void _Linkedlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int temp = ListIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (ListIndex == -1)
                        ListIndex = e.NewStartingIndex + 1;
                    else if (ListIndex >= e.NewStartingIndex || ListIndex == -1)
                        UpdateIndex();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (ListIndex > e.OldStartingIndex)
                        if (ListIndex == e.OldStartingIndex + 1)
                            ListIndex = -1;
                        else
                            UpdateIndex();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    //由外部进行更新
                    break;
            }
            //LogService.DebugWrite($"[{ListIndex}]{e.Action.ToString()} {e.OldStartingIndex} to {e.NewStartingIndex}[{temp}]", nameof(SongItem));
            //不知为何可能会出现ListIndex被初始化为0的情况
            if (ListIndex == 0)
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    Task.Delay(100);
                    UpdateIndex();
                });
        }
        public void UpdateIndex()
        {
            ListIndex = isIncreTtem ? (_LinkedCollection as IList<SongModel>).IndexOf(ItemSource) : _LinkedList.IndexFromContainer(_LinkedItem) + 1;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 获取或设置歌曲信息来源
        /// </summary>
        public SongModel ItemSource
        {
            get { return GetValue(ItemSourceProperty) as SongModel; }
            set { SetValue(ItemSourceProperty, value); }
        }
        private static readonly SongModel _defaultItemSource = default(SongModel);
        /// <summary>
        /// 标识<see cref="ItemSource"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty ItemSourceProperty =
              DependencyProperty.Register(nameof(ItemSource), typeof(SongModel),
                  typeof(SongItem), new PropertyMetadata(_defaultItemSource, (d, e) =>
                  {
                      (d as SongItem).InternalItemSourceChanged(e);
                  }));
        private void InternalItemSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (isIncreTtem)
                UpdateIndex();
            if (PlaylistService.Instance.CurrentPlaying == ItemSource)
                VisualStateManager.GoToState(this, "Playing", true);
        }

        /// <summary>
        /// 获取或设置项目在列表中的位置，从1开始计数
        /// </summary>
        public int ListIndex
        {
            get { return (int)GetValue(ListIndexProperty); }
            set { SetValue(ListIndexProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="ListIndex"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty ListIndexProperty =
              DependencyProperty.Register(nameof(ListIndex), typeof(int),
                  typeof(SongItem), new PropertyMetadata(0));

        /// <summary>
        /// 图标默认的前景色
        /// </summary>
        public Brush NormalIconForeground
        {
            get { return (Brush)GetValue(NormalIconForegroundProperty); }
            set { SetValue(NormalIconForegroundProperty, value); }
        }
        public static readonly DependencyProperty NormalIconForegroundProperty =
            DependencyProperty.Register("NormalIconForeground", typeof(Brush), typeof(SongItem), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        #endregion

        private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ListIndex == 0)
                (_LinkedCollection as IList<SongModel>).RemoveAt(_LinkedList.IndexFromContainer(_LinkedItem));
            if (_LinkedCollection is IList<SongModel>)
                (_LinkedCollection as IList<SongModel>).RemoveAt(ListIndex - 1);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listtype == PlaylistType.LocalPlaylist)
            {
                if (ListIndex == 0) return;
                PlaybackService.Instance.PlayTrack(ListIndex - 1);
            }
            else
                PlaybackService.Instance.PlayTrack(ItemSource);
        }
    }
}
