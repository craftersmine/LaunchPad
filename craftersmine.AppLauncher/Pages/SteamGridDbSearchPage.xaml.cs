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
using craftersmine.AppLauncher.Core;
using craftersmine.AppLauncher.SteamGridDb;
using craftersmine.AppLauncher.SteamGridDb.Api;
using ProgressRing = Microsoft.UI.Xaml.Controls.ProgressRing;

namespace craftersmine.AppLauncher.Pages
{
    public sealed partial class SteamGridDbSearchPage : Page
    {
        private DispatcherTimer searchBoxTimer;
        private const double SearchDelaySeconds = 2;
        private SteamGridDbGame lastSelectedGame = null;
        private bool isLocalStorageUsed;

        public SteamGridDbSearchPage()
        {
            this.InitializeComponent();
            searchBoxTimer = new DispatcherTimer();
            searchBoxTimer.Interval = TimeSpan.FromSeconds(SearchDelaySeconds);
            searchBoxTimer.Stop();
            searchBoxTimer.Tick += SearchBoxTimer_Tick;
            GridViewEmptyListLabel.Visibility = Visibility.Collapsed;
            LoaderRing.Visibility = Visibility.Visible;

            isLocalStorageUsed = true; 
            if (LocalCoverStorage.Instance.LocalCovers.Count == 0)
            {
                NoResultsLabel.Text = "No downloaded covers found! Try searching for game and downloading a couple.";
                RefreshLink.Visibility = Visibility.Collapsed;
                GridViewEmptyListLabel.Visibility = Visibility.Visible;
                LoaderRing.Visibility = Visibility.Collapsed;
            }
            CoversGridView.ItemsSource = LocalCoverStorage.Instance.LocalCovers;
            LoaderRing.Visibility = Visibility.Collapsed;
        }

        private void SearchBoxTimer_Tick(object sender, object e)
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

                isLocalStorageUsed = true;
                CoversGridView.ItemsSource = LocalCoverStorage.Instance.LocalCovers;
                GridViewEmptyListLabel.Visibility = Visibility.Collapsed;
                if (LocalCoverStorage.Instance.LocalCovers.Count == 0)
                {
                    NoResultsLabel.Text = "No downloaded covers found! Try searching for game and downloading a couple.";
                    RefreshLink.Visibility = Visibility.Collapsed;
                    GridViewEmptyListLabel.Visibility = Visibility.Visible;
                    LoaderRing.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void LoadGameCovers(SteamGridDbGame game)
        {
            LoaderRing.Visibility = Visibility.Visible;
            NoResultsLabel.Text = "Unable to retrieve covers for " + lastSelectedGame.Name + ".";
            GridViewEmptyListLabel.Visibility = Visibility.Collapsed;
            lastSelectedGame = game;
            var gridsResults = await SteamGridDb.SteamGridDb.Instance.LoadGridsById(game.Id);

            if (gridsResults is null || !gridsResults.Success)
            {
                NoResultsLabel.Text = "Unable to retrieve covers for \"" + lastSelectedGame.Name + "\".";
                GridViewEmptyListLabel.Visibility = Visibility.Visible;
                RefreshLink.Visibility = Visibility.Visible;
                LoaderRing.Visibility = Visibility.Collapsed;
                return;
            }

            if (gridsResults.Covers.Length == 0)
            {
                NoResultsLabel.Text = "No results found for \"" + lastSelectedGame.Name + "\"";
                GridViewEmptyListLabel.Visibility = Visibility.Visible;
                RefreshLink.Visibility = Visibility.Visible;
                LoaderRing.Visibility = Visibility.Collapsed;
                return;
            }

            LoaderRing.Visibility = Visibility.Collapsed;
            isLocalStorageUsed = false;
            CoversGridView.ItemsSource = gridsResults.Covers;
        }

        private void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            searchBoxTimer.Stop();

            if (!(args.ChosenSuggestion is null))
            {
                if (args.ChosenSuggestion is SteamGridDbGame)
                {
                    CoversGridView.ItemsSource = null;
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

        private async void DownloadCoverButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (isLocalStorageUsed)
            {
                await LocalCoverStorage.Instance.RemoveCoverById((button.DataContext as SteamGridDbGridCover).Id);

                return;
            }

            button.IsEnabled = false;
            button.Content = new ProgressRing()
            {
                Height = 20,
                Width = 20,
                IsActive = true,
                Foreground = (Brush) Application.Current.Resources["AppBarBackgroundThemeBrush"]
            };
            var saved = await LocalCoverStorage.Instance.DownloadCover(button.DataContext as SteamGridDbGridCover);
            if (saved)
            {
                button.Content = new SymbolIcon(Symbol.Accept);
            }
            else
            {
                button.IsEnabled = true;
                button.Content = new SymbolIcon(Symbol.Download);
            }
        }

        private void AvatarOrCoverImageOpened(object sender, RoutedEventArgs e)
        {
            ((sender as Image).Tag as ProgressRing).Visibility = Visibility.Collapsed;
        }

        private void CoversGridView_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {

            if (isLocalStorageUsed && LocalCoverStorage.Instance.LocalCovers.Count == 0)
            {
                NoResultsLabel.Text = "No downloaded covers found! Try searching for game and downloading a couple.";
                RefreshLink.Visibility = Visibility.Collapsed;
                GridViewEmptyListLabel.Visibility = Visibility.Visible;
            }

            if (args.ItemContainer.ContentTemplateRoot is null)
                return;
            
            var downloadButton = 
                (((args.ItemContainer.ContentTemplateRoot as Grid).Children.FirstOrDefault(i => i is StackPanel) as StackPanel)
                .Children.FirstOrDefault(i => i is Grid) as Grid)
                .Children.FirstOrDefault(i=> i is Button) as Button;

            if (isLocalStorageUsed)
            {
                downloadButton.Content = new SymbolIcon(Symbol.Delete);
                downloadButton.Tag = args.Item;
            }
            else
            {
                downloadButton.Tag = null;
                downloadButton.Content = new SymbolIcon(Symbol.Download);
            }

            LoaderRing.Visibility = Visibility.Collapsed;
        }
    }
}
