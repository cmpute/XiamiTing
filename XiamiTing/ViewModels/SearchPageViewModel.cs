using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;
using JacobC.Xiami.Models;

namespace JacobC.Xiami.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        ObservableCollection<Tuple<XiamiModelBase, SearchResultType>> Suggestion = new ObservableCollection<Tuple<XiamiModelBase, SearchResultType>>();

        public async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(sender.Text))
                Suggestion.Clear();
            else if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var result = await Net.WebApi.Instance.SearchBrief(sender.Text);
                Suggestion.Clear();
                if (result.Songs != null)
                    foreach (var item in result.Songs)
                        Suggestion.Add(new Tuple<XiamiModelBase, SearchResultType>(item, SearchResultType.Song));
                if (sender.ItemsSource != Suggestion)
                    sender.ItemsSource = Suggestion;
            }
        }
        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        enum SearchResultType
        {
            Match,
            Song,
            Album,
            Artist
        }
    }
}
