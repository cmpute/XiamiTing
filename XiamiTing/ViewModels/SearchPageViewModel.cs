using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;
using JacobC.Xiami.Models;
using JacobC.Xiami.Views;
using Windows.UI.Xaml.Navigation;

namespace JacobC.Xiami.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        ObservableCollection<XiamiModelBase> Suggestion = new ObservableCollection<XiamiModelBase>();

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
                await Search(parameter.ToString());
        }

        public async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(sender.Text))
                Suggestion.Clear();
            else if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var result = await Net.WebApi.Instance.SearchBrief(sender.Text);
                Suggestion.Clear();
                if (result.Songs != null)
                {
                    foreach (var item in result.Songs)
                        Suggestion.Add(item);
                    Suggestion.Add(SearchResultSelector.Separator);
                }
                if (result.Albums != null)
                {
                    foreach (var item in result.Albums)
                        Suggestion.Add(item);
                    Suggestion.Add(SearchResultSelector.Separator);
                }
                if (result.Artists != null)
                {
                    foreach (var item in result.Artists)
                        Suggestion.Add(item);
                    Suggestion.Add(SearchResultSelector.Separator);
                }
                if (sender.ItemsSource != Suggestion)
                    sender.ItemsSource = Suggestion;
            }
        }
        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem == SearchResultSelector.Separator)
                sender.Text = "";
            else
            {
                sender.Text = (args.SelectedItem as XiamiModelBase).Name;
                if (args.SelectedItem is SongModel)
                    Services.PlaybackService.Instance.PlayTrack(args.SelectedItem as SongModel);
                if (args.SelectedItem is AlbumModel)
                    NavigationService.NavigateAsync(typeof(AlbumPage), (args.SelectedItem as AlbumModel).XiamiID);
                if (args.SelectedItem is ArtistModel)
                    NavigationService.NavigateAsync(typeof(ArtistPage), (args.SelectedItem as ArtistModel).XiamiID);
            }
        }
        public async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            sender.IsSuggestionListOpen = false;
            await Search(args.QueryText);
        }


        public async Task Search(string keyword)
        {
            SearchResult result = await Net.WebApi.Instance.Search(keyword);
            Dispatcher.Dispatch(() => SearchingFinished?.Invoke(this, result));
        }

        public event EventHandler<SearchResult> SearchingFinished;
    }
}
