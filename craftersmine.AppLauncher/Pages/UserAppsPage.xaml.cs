using craftersmine.AppLauncher.Core;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace craftersmine.AppLauncher.Pages
{
    public sealed partial class UserAppsPage : Page
    {
        public static ICommand EditCommand { get; set; }

        public UserAppsPage()
        {
            this.InitializeComponent();
        }

        private void appList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var frame = ((Frame)Parent);
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            frame.Navigate(typeof(UserAppPage), e.ClickedItem);
            navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
            navView.IsBackEnabled = true;
        }

        private void gridView_context_editApp_Click(object sender, RoutedEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext as UserApp;
            var frame = ((Frame)Parent);
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            frame.Navigate(typeof(UserAppEditor), item);
            navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
            navView.IsBackEnabled = true;
        }

        private async void gridView_context_deleteApp_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog removeDlg = new ContentDialog();
            removeDlg.Title = ResourceManagers.StringsUserAppInfoResources.GetString("RemoveDialog_Title");
            removeDlg.Content = ResourceManagers.StringsUserAppInfoResources.GetString("RemoveDialog_Content");
            removeDlg.PrimaryButtonText = ResourceManagers.StringsCommonResources.GetString("Yes");
            removeDlg.CloseButtonText = ResourceManagers.StringsCommonResources.GetString("No");
            removeDlg.DefaultButton = ContentDialogButton.Close;
            removeDlg.PrimaryButtonClick += RemoveDlg_PrimaryButtonClick;
            removeDlg.Tag = (e.OriginalSource as FrameworkElement).DataContext as UserApp;
            await removeDlg.ShowAsync();
        }

        private void appList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (AppList.Items?.Count > 0)
                GridViewEmptyListLabel.Visibility = Visibility.Collapsed;
            else
                GridViewEmptyListLabel.Visibility = Visibility.Visible;

        }

        private void button_addItem_Click(object sender, RoutedEventArgs e)
        {
            var frame = ((Frame)Parent);
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            frame.Navigate(typeof(UserAppEditor), null);
            navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
            navView.IsBackEnabled = true;
        }

        private void searchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!(args.ChosenSuggestion is null) && !(args.ChosenSuggestion is string))
            {
                var frame = ((Frame)Parent);
                var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
                frame.Navigate(typeof(UserAppPage), args.ChosenSuggestion);
                navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
                navView.IsBackEnabled = true;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(args.QueryText))
                {
                    ObservableCollection<UserApp> searched = new ObservableCollection<UserApp>();
                    foreach (var item in AppManager.Apps.Where<UserApp>(i => i.Name.ToLower().Contains(args.QueryText.ToLower())))
                    {
                        searched.Add(item);
                    }
                    AppList.ItemsSource = searched;
                }
                else
                {
                    AppList.ItemsSource = AppManager.Apps;
                }
            }
        }

        private void searchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suitableItems = new List<UserApp>();
                var splitText = sender.Text.ToLower().Split(" ");

                foreach (UserApp app in AppManager.Apps)
                {
                    var found = splitText.All((key) => app.Name.ToLower().Contains(key));
                    if (found)
                        suitableItems.Add(app);
                }

                if (suitableItems.Count == 0)
                {
                    List<string> noResults = new List<string> { "No results found" };  // TODO: Refactor string in resources for localization
                    sender.ItemsSource = noResults;
                    return;
                }

                sender.ItemsSource = suitableItems;

                if (string.IsNullOrWhiteSpace(sender.Text))
                {
                    AppList.ItemsSource = AppManager.Apps;
                    return;
                }

            }
        }

        private void gridView_appItem_Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            (sender as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/NoCover.png"));
        }

        private void gridView_context_launchApp_Click(object sender, RoutedEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext as UserApp;
            AppManager.LaunchApp(item);
        }

        private void GridView_appItem_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var textGrid = ((Grid) sender).Children.FirstOrDefault(c => c.GetType() == typeof(StackPanel)) as StackPanel;
            textGrid.Height = 120;
            var descTextBlock = textGrid.Children.FirstOrDefault(c =>
                c.GetType() == typeof(TextBlock) && (c as TextBlock).Name.Contains("Description")) as TextBlock;
            descTextBlock.TextWrapping = TextWrapping.WrapWholeWords;
        }

        private void GridView_appItem_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var textGrid = ((Grid)sender).Children.FirstOrDefault(c => c.GetType() == typeof(StackPanel)) as StackPanel;
            textGrid.Height = 60;
            var descTextBlock = textGrid.Children.FirstOrDefault(c =>
                c.GetType() == typeof(TextBlock) && (c as TextBlock).Name.Contains("Description")) as TextBlock;
            descTextBlock.TextWrapping = TextWrapping.NoWrap;
        }

        private async void AppList_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            await AppManager.RequestListSave();
        }

        private async void RemoveDlg_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var item = sender.Tag as UserApp;
            AppManager.Apps.Remove(item);
            await AppManager.RequestListSave();
        }
    }
}
