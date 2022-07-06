using craftersmine.AppLauncher.Pages;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

using Microsoft.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI;
using Windows.UI.Core;
using craftersmine.AppLauncher.Core;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;


// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace craftersmine.AppLauncher
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            SettingsManager.Instance.Apply();

            if (FrameRoot.Content is null)
                FrameRoot.Navigate(typeof(UserAppsPage));

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.InactiveForegroundColor = Colors.Gray;

            Window.Current.SetTitleBar(AppTitlebar);

            AppManager.ApplicationEvent += AppManager_ApplicationEvent;
        }

        private async void AppManager_ApplicationEvent(object sender, ApplicationEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,  async () => {
            AppStatusDialog.Visibility = Visibility.Visible;
            switch (e.ApplicationEventType)
            {
                case ApplicationEventType.Launching:
                    AppStatusTextBlock.Text = string.Format(ResourceManagers.StringsCommonResources.GetString("AppStatusTextBlock_Launching"), AppManager.GetApplicationByUuid(e.AppUuid).Name);
                    await AppStatusDialog.ShowAsync();
                    break;
                case ApplicationEventType.Launched:
                    AppStatusDialog.IsPrimaryButtonEnabled = true;
                    AppStatusDialog.IsSecondaryButtonEnabled = true;
                    AppStatusTextBlock.Text = string.Format(ResourceManagers.StringsCommonResources.GetString("AppStatusTextBlock_Launched"), AppManager.GetApplicationByUuid(e.AppUuid).Name);
                    break;
                case ApplicationEventType.Exited:
                    AppStatusTextBlock.Text = string.Format(ResourceManagers.StringsCommonResources.GetString("AppStatusTextBlock_Closed"), AppManager.GetApplicationByUuid(e.AppUuid).Name, e.ExitCode);
                    await Task.Delay(SettingsManager.Instance.ApplicationExitedWaitTime);
                    HideStatusDialog();
                    break;
                case ApplicationEventType.MissingExecutable:
                    HideStatusDialog();
                    ShowMissingFileOrDirDialog(e.AppUuid, false);
                    break;
                case ApplicationEventType.MissingWorkingDirectory:
                    HideStatusDialog();
                    ShowMissingFileOrDirDialog(e.AppUuid, true);
                    break;
                }
            });
        }

        private void HideStatusDialog()
        {
            AppStatusDialog.Hide();
            AppStatusTextBlock.Text = "";
            AppStatusDialog.IsPrimaryButtonEnabled = false;
            AppStatusDialog.IsSecondaryButtonEnabled = false;
        }

        private async void ShowMissingFileOrDirDialog(string appUuid, bool isWorkingDir)
        {

            ContentDialog missingExecutableContentDialog = new ContentDialog();
                missingExecutableContentDialog.DefaultButton = ContentDialogButton.Close;
            missingExecutableContentDialog.CloseButtonText =
                ResourceManagers.StringsCommonResources.GetString("Ok");
            missingExecutableContentDialog.PrimaryButtonText =
                ResourceManagers.StringsCommonResources.GetString(
                    "UnableToFindFileOrDirDlg_OpenApplicationEditorButton");
            missingExecutableContentDialog.Tag = appUuid;
            missingExecutableContentDialog.PrimaryButtonClick += MissingExecutableContentDialog_PrimaryButtonClick;
            missingExecutableContentDialog.Title = ResourceManagers.StringsCommonResources.GetString("UnableToFindFileOrDirDlg_Title");
            if (isWorkingDir)
                missingExecutableContentDialog.Content = string.Format(ResourceManagers.StringsCommonResources.GetString("UnableToFindDirDlg_Content"),
                    AppManager.GetApplicationByUuid(appUuid).Name, AppManager.GetApplicationByUuid(appUuid).WorkingDirectory);
            if (!isWorkingDir)
                missingExecutableContentDialog.Content = string.Format(ResourceManagers.StringsCommonResources.GetString("UnableToFindFileDlg_Content"),
                    AppManager.GetApplicationByUuid(appUuid).Name, AppManager.GetApplicationByUuid(appUuid).ExecutablePath);
            await missingExecutableContentDialog.ShowAsync();
        }

        private void MissingExecutableContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FrameRoot.Navigate(typeof(UserAppEditor), AppManager.GetApplicationByUuid((string)sender.Tag));
            NavigationView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
            NavigationView.IsBackEnabled = true;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            switch (args.SelectedItemContainer.Name.ToLower())
            {
                case "menuhome":
                    FrameRoot.Navigate(typeof(UserAppsPage));
                    break;
                case "settingsitem":
                    FrameRoot.Navigate(typeof(SettingsPage), null, new EntranceNavigationTransitionInfo());
                    break;
                case "steamgriddblibrary":
                    FrameRoot.Navigate(typeof(SteamGridDbSearchPage));
                    break;
                default:
                    break;
            }

            FrameRoot.BackStack.Clear();
            sender.IsBackEnabled = false;
            sender.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            FrameRoot.Navigate(typeof(UserAppsPage));
            FrameRoot.BackStack.Clear();
            sender.IsBackEnabled = false;
            sender.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }

        private void navigationView_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppManager.IsFailedToLoadList)
            {
                InfoBadge attentionBadge = new InfoBadge();
                attentionBadge.Style = (Style)Application.Current.Resources["AttentionIconInfoBadgeStyle"];
                ((NavigationViewItem)NavigationView.SettingsItem).InfoBadge = attentionBadge;
            }
        }

        private void appStatusDialog_PrimaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, Windows.UI.Xaml.Controls.ContentDialogButtonClickEventArgs args)
        {
            AppManager.SendLaunchedAppClose();
            args.Cancel = true;
        }

        private void appStatusDialog_SecondaryButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, Windows.UI.Xaml.Controls.ContentDialogButtonClickEventArgs args)
        {
            AppManager.SendLaunchedAppTerminate();
            args.Cancel = true;
        }

        private void appStatusDialog_CloseButtonClick(Windows.UI.Xaml.Controls.ContentDialog sender, Windows.UI.Xaml.Controls.ContentDialogButtonClickEventArgs args)
        {
            AppManager.SendBridgeExit();
        }
    }
}
