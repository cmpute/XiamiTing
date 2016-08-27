using JacobC.Xiami.Models;
using JacobC.Xiami.Services;
using JacobC.Xiami.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Mvvm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data ;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using JacobC.Xiami.ViewModels;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace JacobC.Xiami.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistPage()
        {
            this.InitializeComponent();
            //TODO: 移到ViewModel里面
            Songlist.SongSource = PlaylistService.Instance;
            Songlist.SelectionUpdated += listView_SelectionChanged;
        }

        private DelegateCommand<object> _DeleteCommand;
        public DelegateCommand<object> DeleteCommand => _DeleteCommand ?? (_DeleteCommand = new DelegateCommand<object>((model) =>
        {
            Songlist.OperateSelectedItems(SelectionOperation.Delete);
        }, (model) => { return Songlist.CanOperateItem(SelectionOperation.Delete); }));

        private DelegateCommand<object> _MultipleSelect;
        public DelegateCommand<object> MultipleSelect => _MultipleSelect ?? (_MultipleSelect = new DelegateCommand<object>((model) =>
        {
            if (Songlist.SelectionMode == ListViewSelectionMode.Extended)
            {
                Songlist.SelectionMode = ListViewSelectionMode.Multiple;
                MultiSelect.Label = "SingleSelect";
                SelectAll.Visibility = Visibility.Visible;
            }
            else if(Songlist.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Songlist.SelectionMode = ListViewSelectionMode.Extended;
                MultiSelect.Label = "MultiSelect";
                SelectAll.Visibility = Visibility.Collapsed;
            }
        }));

        private DelegateCommand<object> _SelectAll;
        public DelegateCommand<object> SelectAllCommand => _SelectAll ?? (_SelectAll = new DelegateCommand<object>((model) =>
        {
            Songlist.OperateSelectedItems(SelectionOperation.SelectAll);
        }));

        private void listView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _DeleteCommand.RaiseCanExecuteChanged();
        }
    }
}
