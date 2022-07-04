using craftersmine.AppLauncher.Core;

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
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;
using SplitButtonClickEventArgs = Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs;

namespace craftersmine.AppLauncher.Pages
{
    public sealed partial class UserAppPage : Page
    {
        public UserApp SelectedApp { get; private set; } = null;

        public string LaunchArguments
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedApp.LaunchArguments))
                    return ResourceManagers.StringsCommonResources.GetString("None");
                return SelectedApp.LaunchArguments;
            }
        }

        public string WorkingDir
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedApp.WorkingDirectory))
                    return ResourceManagers.StringsCommonResources.GetString("Default");
                return SelectedApp.WorkingDirectory;
            }
        }

        public string RunAsAdmin
        {
            get
            {
                if (SelectedApp.RunAsAdmin)
                    return ResourceManagers.StringsCommonResources.GetString("Yes");
                return ResourceManagers.StringsCommonResources.GetString("No");
            }
        }

        public UserAppPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SelectedApp = e.Parameter as UserApp;

            base.OnNavigatedTo(e);
        }

        private void LaunchButton_OnClick(SplitButton sender, SplitButtonClickEventArgs args)
        {
            AppManager.LaunchApp(SelectedApp);
        }

        private void EditButtonFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            var frame = ((Frame)Parent);
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            frame.Navigate(typeof(UserAppEditor), SelectedApp);
            navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Visible;
            navView.IsBackEnabled = true;
        }

        private async void RemoveButtonFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            ContentDialog removeDlg = new ContentDialog();
            removeDlg.Title = ResourceManagers.StringsUserAppInfoResources.GetString("RemoveDialog_Title");
            removeDlg.Content = ResourceManagers.StringsUserAppInfoResources.GetString("RemoveDialog_Content");
            removeDlg.PrimaryButtonText = ResourceManagers.StringsCommonResources.GetString("Yes");
            removeDlg.CloseButtonText = ResourceManagers.StringsCommonResources.GetString("No");
            removeDlg.CloseButtonStyle = (Style) Application.Current.Resources["AccentButtonStyle"];
            removeDlg.PrimaryButtonClick += RemoveDlg_PrimaryButtonClick;
            await removeDlg.ShowAsync();
        }

        private async void RemoveDlg_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            AppManager.Apps.Remove(SelectedApp);
            await AppManager.RequestListSave();

            var frame = ((Frame)Parent);
            frame.Navigate(typeof(UserAppsPage));
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            frame.BackStack.Clear();
            navView.IsBackEnabled = false;
            navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed;
        }
    }
}
