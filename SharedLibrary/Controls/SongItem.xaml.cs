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
            this.Loaded += SongItem_Loaded;
            this.Unloaded += SongItem_Unloaded;
        }

        #region Link to Parent

        VisualStateGroup _LinkedGroup = null;
        DependencyObject _LinkedItem = null;
        ItemsControl _LinkedList = null;
        INotifyCollectionChanged _LinkedCollection = null;
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
                    _LinkedList = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
                    ListIndex = _LinkedList.IndexFromContainer(item) + 1;
                    if(_LinkedList.ItemsSource is INotifyCollectionChanged)
                    {
                        _LinkedCollection = _LinkedList.ItemsSource as INotifyCollectionChanged;
                        _LinkedCollection.CollectionChanged += _Linkedlist_CollectionChanged;
                    }
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
            _LinkedList = null;
            _LinkedItem = null;
        }
        private void _Linkedlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int temp = ListIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (ListIndex == -1)
                        ListIndex = e.NewStartingIndex + 1;
                    else if (ListIndex >= e.NewStartingIndex || ListIndex == -1)
                        ListIndex = _LinkedList.IndexFromContainer(_LinkedItem) + 1;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (ListIndex > e.OldStartingIndex)
                        if (ListIndex == e.OldStartingIndex + 1)
                            ListIndex = -1;
                        else
                            ListIndex = _LinkedList.IndexFromContainer(_LinkedItem) + 1;
                    break;
            }
            //LogService.DebugWrite($"[{ListIndex}]{e.Action.ToString()} {e.OldStartingIndex} to {e.NewStartingIndex}[{temp}]", nameof(SongItem));
            //不知为何可能会出现ListIndex被初始化为0的情况
            if (ListIndex == 0)
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    Task.Delay(100);
                    ListIndex = _LinkedList.IndexFromContainer(_LinkedItem) + 1;
                });
        }

        #endregion

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


        public Brush NormalIconForeground
        {
            get { return (Brush)GetValue(NormalIconForegroundProperty); }
            set { SetValue(NormalIconForegroundProperty, value); }
        }
        public static readonly DependencyProperty NormalIconForegroundProperty =
            DependencyProperty.Register("NormalIconForeground", typeof(Brush), typeof(SongItem), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ListIndex == 0)
                (_LinkedCollection as Collection<SongModel>).RemoveAt(_LinkedList.IndexFromContainer(_LinkedItem));
            if (_LinkedCollection is Collection<SongModel>)
                (_LinkedCollection as Collection<SongModel>).RemoveAt(ListIndex - 1); ;
        }
    }
}
