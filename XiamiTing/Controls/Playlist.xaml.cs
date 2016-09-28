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
using JacobC.Xiami.Models;
using JacobC.Xiami.Services;

namespace JacobC.Xiami.Controls
{
    public sealed partial class Playlist : UserControl
    {
        public Playlist()
        {
            this.InitializeComponent();
            Loaded += (sender, e) => PlaylistService.Instance.CurrentIndexChanged += Instance_CurrentIndexChanged;
            Songlist.SelectionChanged += (sender, e) => SelectionUpdated?.Invoke(sender, e);
        }

        private void Instance_CurrentIndexChanged(object sender, Template10.Common.ChangedEventArgs<int> e)
        {
            //TODO: 针对播放中的打乱顺序进行处理
            ListViewItem t;
            if (ListType == PlaylistType.LocalPlaylist)
            {
                if (e.OldValue >= 0)
                {
                    t = Songlist.ContainerFromIndex(e.OldValue) as ListViewItem;
                    VisualStateManager.GoToState((t.Tag as SongItem), "NotPlaying", true);
                }
                if (e.NewValue >= 0)
                {
                    t = Songlist.ContainerFromIndex(e.NewValue) as ListViewItem;
                    VisualStateManager.GoToState((t.Tag as SongItem), "Playing", true);
                    //TODO: 增加新专辑的时候会存在Item不为空但是container为空的情况
                }
            }
            else
            {
                //TODO: 刷新所有歌曲的播放状态？
            }
        }

        #region Public Properties
        /// <summary>
        /// 获取或设置播放列表的类型
        /// </summary>
        public PlaylistType ListType
        {
            get { return (PlaylistType)GetValue(ListTypeProperty); }
            set { SetValue(ListTypeProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="ListType"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty ListTypeProperty =
              DependencyProperty.Register(nameof(ListType), typeof(PlaylistType),
                  typeof(Playlist), new PropertyMetadata(PlaylistType.LocalPlaylist, (d, e) =>
                  {
                      (d as Playlist).InternalListTypeChanged(e);
                      (d as Playlist).ListTypeChanged?.Invoke(d,e);
                  }));
        /// <summary>
        /// 当<see cref="ListType"/>发生改变时发生
        /// </summary>
        public event EventHandler<DependencyPropertyChangedEventArgs> ListTypeChanged;
        private void InternalListTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            //TODO:完成模式更改的更新
            Songlist.Tag = e.NewValue;
            switch((PlaylistType)(e.NewValue))
            {
                case PlaylistType.LocalPlaylist:
                    //SelectionMode = ListViewSelectionMode.Extended;
                    break;
                case PlaylistType.AlbumPlaylist:
                    //SelectionMode = ListViewSelectionMode.Extended;
                    Songlist.CanReorderItems = false;
                    break;
                case PlaylistType.CollectionPlaylist:
                    Songlist.CanReorderItems = false;
                    break;
            }
        }


        /// <summary>
        /// 获取或设置列表的歌曲来源
        /// </summary>
        public object SongSource
        {
            get { return (object)GetValue(SongSourceProperty); }
            set { SetValue(SongSourceProperty, value); }
        } 
        public static readonly DependencyProperty SongSourceProperty =
            DependencyProperty.Register("SongSource", typeof(object), typeof(Playlist), new PropertyMetadata(null));

        /// <summary>
        /// 获取或设置列表的选择模式属性
        /// </summary>
        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }
        /// <summary>
        /// 标识<see cref="SelectionMode"/>依赖属性
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty =
              DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode),
                  typeof(Playlist), new PropertyMetadata(ListViewSelectionMode.Extended, (d, e) =>
                  {
                      //(d as Playlist).SelectionModeChanged?.Invoke(d, e.ToChangedEventArgs<SelectionMode>());
                      (d as Playlist).InternalSelectionModeChanged(e);
                  }));
        /// <summary>
        /// 在<see cref="SelectionMode"/>属性发生变更时发生
        /// </summary>
        //public event EventHandler<ChangedEventArgs<ListViewSelectionMode>> SelectionModeChanged;
        private void InternalSelectionModeChanged(DependencyPropertyChangedEventArgs e)
        {
            //TODO: 改变标题栏的结构
        }
        #endregion

        #region Operations
        /// <summary>
        /// 对选中歌曲项进行操作
        /// </summary>
        /// <param name="operation">操作内容</param>
        public void OperateSelectedItems(SelectionOperation operation)
        {
            Action<SongModel> action = null;
            switch (operation)
            {
                case SelectionOperation.Delete:
                    while (Songlist.SelectedIndex >= 0)
                        (Songlist.ItemsSource as IList<SongModel>).RemoveAt(Songlist.SelectedIndex);
                    return;
                case SelectionOperation.Love:
                    action = model => model.IsLoved = true; // TODO: 在这更改设置的时候会抛出错误
                    break;
                case SelectionOperation.SelectAll:
                    Songlist.SelectAll();
                    return;
                case SelectionOperation.SelectOther:
                    OperateAllItems((item) => item.IsSelected = !item.IsSelected);
                    return;
            }
            foreach (var item in Songlist.SelectedItems)
                action?.Invoke(item as SongModel);
        }
        /// <summary>
        /// 判断是否允许对歌曲项进行操作
        /// </summary>
        /// <param name="operation">操作内容</param>
        public bool CanOperateItem(SelectionOperation operation)
        {
            if (operation == SelectionOperation.SelectAll || operation == SelectionOperation.SelectOther)
                return true;
            if (Songlist.SelectedIndex < 0)
                return false;
            switch (operation)
            {
                //case SelectionOperation.Delete:
                //    return Songlist.SelectedIndex > 0;
                //case SelectionOperation.Love:
                //    return true;
                default:
                    return true;
            }
        }
        /// <summary>
        /// 更新所有项目的索引
        /// </summary>
        public void UpdateItemsIndex()
        {
            OperateAllItems((item) =>
            {
                if (item.Tag is SongItem)
                    (item.Tag as SongItem).UpdateIndex();
            });
        }
        private void OperateAllItems(Action<ListViewItem> action)
        {
            for (int i = 0; i < Songlist.Items.Count; i++)
            {
                var item = Songlist.ContainerFromIndex(i) as ListViewItem;
                if (item == null)
                    continue;
                action.Invoke(item);
            }
        }
        #endregion

        public event RoutedEventHandler SelectionUpdated;
    }


    /// <summary>
    /// 标识对选中播放列表项的操作类型
    /// </summary>
    public enum SelectionOperation
    {
        /// <summary>
        /// 从列表中删除选中歌曲
        /// </summary>
        Delete,
        /// <summary>
        /// 全选
        /// </summary>
        SelectAll,
        /// <summary>
        /// 反选
        /// </summary>
        SelectOther,
        //UnSelectAll, //取消全部选择(通过全选+反选或者切换选择模式可以完成)
        /// <summary>
        /// 将选中歌曲顺序颠倒
        /// </summary>
        UpSideDown,

        /// <summary>
        /// 收藏选中歌曲
        /// </summary>
        Love,
        /// <summary>
        /// 添加到精选集
        /// </summary>
        AddToCollection,
        /// <summary>
        /// 添加到本地播放列表
        /// </summary>
        AddToPlaylist,
        /// <summary>
        /// 下载选中歌曲
        /// </summary>
        Download
    }

    /// <summary>
    /// 标识播放列表的类别
    /// </summary>
    public enum PlaylistType
    {
        /// <summary>
        /// 专辑的曲目列表
        /// </summary>
        AlbumPlaylist,
        /// <summary>
        /// 精选集歌曲列表
        /// </summary>
        CollectionPlaylist,
        /// <summary>
        /// 本地播放列表
        /// </summary>
        LocalPlaylist
    }
}

