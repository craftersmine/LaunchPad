using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using craftersmine.AppLauncher.SteamGridDb;
using craftersmine.AppLauncher.SteamGridDb.Api;
using ProgressRing = Microsoft.UI.Xaml.Controls.ProgressRing;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace craftersmine.AppLauncher.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SteamGridDbSearchPage : Page
    {
        private DispatcherTimer searchBoxTimer;
        private const double SearchDelaySeconds = 2;
        private SteamGridDbGame lastSelectedGame = null;

        public SteamGridDbSearchPage()
        {
            this.InitializeComponent();
            searchBoxTimer = new DispatcherTimer();
            searchBoxTimer.Interval = TimeSpan.FromSeconds(SearchDelaySeconds);
            searchBoxTimer.Stop();
            searchBoxTimer.Tick += SearchBoxTimer_Tick;
            GridViewEmptyListLabel.Visibility = Visibility.Collapsed;

            // TODO: Load local stored library
        }

        private async void SearchBoxTimer_Tick(object sender, object e)
        {
            searchBoxTimer.Stop();
            searchBoxTimer.Interval = TimeSpan.FromSeconds(SearchDelaySeconds);
            PerformSearch();
        }

        private async void PerformSearch()
        {
            var searchResults = await SteamGridDb.SteamGridDb.Instance.SearchForGamesByName(SearchBox.Text);
            if (!(searchResults is null) && searchResults.Success)
            {
                if (searchResults.Data.Length > 0)
                    SearchBox.ItemsSource = searchResults.Data;
            }
            else
            {
                SearchBox.ItemsSource = new string[] { "No results found" };
            }
        }

        private void SearchBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (searchBoxTimer.IsEnabled)
                    searchBoxTimer.Stop();
                searchBoxTimer.Start();
            }

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.ItemsSource = null;
                searchBoxTimer.Stop();

                // TODO: Load local stored library
                GridViewEmptyListLabel.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadGameCovers(SteamGridDbGame game)
        {
            NoResultsLabel.Text = "Unable to retrieve covers for " + lastSelectedGame.Name + ".";
            GridViewEmptyListLabel.Visibility = Visibility.Collapsed;
            lastSelectedGame = game;
            var gridsResults = await SteamGridDb.SteamGridDb.Instance.LoadGridsById(game.Id);

            if (gridsResults is null || !gridsResults.Success)
            {
                NoResultsLabel.Text = "Unable to retrieve covers for \"" + lastSelectedGame.Name + "\".";
                GridViewEmptyListLabel.Visibility = Visibility.Visible;
                return;
            }

            if (gridsResults.Covers.Length == 0)
            {
                NoResultsLabel.Text = "No results found for \"" + lastSelectedGame.Name + "\"";
                GridViewEmptyListLabel.Visibility = Visibility.Visible;
                return;
            }

            CoversGridView.ItemsSource = gridsResults.Covers;
        }

        private void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            searchBoxTimer.Stop();

            if (!(args.ChosenSuggestion is null))
            {
                if (args.ChosenSuggestion is SteamGridDbGame)
                {
                    lastSelectedGame = args.ChosenSuggestion as SteamGridDbGame;
                    LoadGameCovers(args.ChosenSuggestion as SteamGridDbGame);
                    return;
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(args.QueryText))
            {
                PerformSearch();
            }
        }

        private void AvatarOrCoverImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            (sender as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/NoCover.png"));
            ((sender as Image).Tag as ProgressRing).Visibility = Visibility.Collapsed;
        }

        private void RefreshListClick(object sender, RoutedEventArgs e)
        {
            LoadGameCovers(lastSelectedGame);
        }

        private void GridElement_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var textGrid = ((Grid)sender).Children.FirstOrDefault(c => c.GetType() == typeof(StackPanel)) as StackPanel;
            textGrid.Height = 110;
        }

        private void GridElement_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var textGrid = ((Grid)sender).Children.FirstOrDefault(c => c.GetType() == typeof(StackPanel)) as StackPanel;
            textGrid.Height = 60;
        }

        private async void AuthorLinkClicked(object sender, RoutedEventArgs e)
        {
            string authorSteamId = (sender as HyperlinkButton).Tag as string;

            await Launcher.LaunchUriAsync(new Uri("https://steamcommunity.com/profiles/" + authorSteamId));
        }

        private void DownloadCoverButtonClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void AvatarOrCoverImageOpened(object sender, RoutedEventArgs e)
        {
            ((sender as Image).Tag as ProgressRing).Visibility = Visibility.Collapsed;
        }
    }
}
