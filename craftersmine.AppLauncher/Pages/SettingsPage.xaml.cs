using craftersmine.AppLauncher.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace craftersmine.AppLauncher.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            SoundSwitch.IsOn = SettingsManager.Instance.EnableSound;
            LockDialogTimeout.Value = SettingsManager.Instance.ApplicationExitedWaitTime;

            var languages = SettingsManager.GetAvailableLanguages();
            foreach (var lang in languages)
            {
                LanguagesComboBox.Items?.Add(lang);
            }

            LanguagesComboBox.SelectedItem = LanguagesComboBox.Items?.FirstOrDefault(l => (l as Language)?.LanguageTag == SettingsManager.Instance.Language);

            switch (SettingsManager.Instance.AppTheme)
            {
                case AppTheme.Dark:
                    ThemeDarkRadio.IsChecked = true;
                    break;
                case AppTheme.Light:
                    ThemeLightRadio.IsChecked = true;
                    break;
                case AppTheme.UseSystemSetting:
                default:
                    ThemeUseSystemRadio.IsChecked= true;
                    break;
            }

            if (AppManager.IsFailedToLoadList)
                ErrorBar.Visibility = Visibility.Visible;

            if (AppManager.HasBrokenAppList)
            {
                ErrorBar.Visibility = Visibility.Visible;
                ErrorBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                ErrorBar.Title = ResourceManagers.StringsSettingsResources.GetString("AppListErrorOld_Title");
            }
        }

        private void soundSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.EnableSound = SoundSwitch.IsOn;
            SettingsManager.Instance.Save();
            SettingsManager.Instance.Apply();
        }

        private void runApp_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void InfoBar_CloseButtonClick(Microsoft.UI.Xaml.Controls.InfoBar sender, object args)
        {
            sender.IsOpen = true;
            sender.Visibility = Visibility.Visible;
            var brokenAppList = await ApplicationData.Current.LocalFolder.TryGetItemAsync("AppList.broken.xml") as StorageFile;
            if (!(brokenAppList is null))
            {
                ContentDialog errorDialog = new ContentDialog();
                errorDialog.Title = ResourceManagers.StringsSettingsResources.GetString("AppListErrorDlgOld_Title");
                errorDialog.Content = ResourceManagers.StringsSettingsResources.GetString("AppListErrorDlgOld_Content");
                errorDialog.CloseButtonText = ResourceManagers.StringsCommonResources.GetString("No"); ;
                errorDialog.SecondaryButtonText = ResourceManagers.StringsCommonResources.GetString("Yes"); ;
                errorDialog.CloseButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];
                errorDialog.SecondaryButtonClick += ErrorDialog_SecondaryButtonClick;
                await errorDialog.ShowAsync();
                return;
            }
        }

        private async void ErrorDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var brokenAppList = await ApplicationData.Current.LocalFolder.TryGetItemAsync("AppList.broken.xml") as StorageFile;
            if (!(brokenAppList is null))
                await brokenAppList.DeleteAsync();

            var frame = ((Frame)Parent);
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            ((Microsoft.UI.Xaml.Controls.NavigationViewItem)navView.SettingsItem).InfoBadge = null;
            ErrorBar.Visibility = Visibility.Collapsed;
            AppManager.DismissErrors();
        }

        private async void saveOldAppList_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.FileTypeChoices.Add(ResourceManagers.StringsSettingsResources.GetString("FileTypes_Xml"), new string[] { ".xml" });
            fileSavePicker.SuggestedFileName = "AppList.broken.xml";
            fileSavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fileSavePicker.CommitButtonText = ResourceManagers.StringsSettingsResources.GetString("Picker_Save_AppList"); ;
            var file = await fileSavePicker.PickSaveFileAsync();

            var brokenAppList = await ApplicationData.Current.LocalFolder.TryGetItemAsync("AppList.broken.xml") as StorageFile;
            if (brokenAppList is null)
            {
                ContentDialog errorDialog = new ContentDialog();
                errorDialog.Content = ResourceManagers.StringsSettingsResources.GetString("AppListSaveError_Content"); ;
                errorDialog.Title = ResourceManagers.StringsSettingsResources.GetString("AppListSaveError_Title"); ;
                errorDialog.CloseButtonText = "OK";
                errorDialog.CloseButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];
                await errorDialog.ShowAsync();
                return;
            }
            if (file is null)
                return;

            await brokenAppList.MoveAndReplaceAsync(file);
            ErrorBar.IsOpen = false;
            ErrorBar.Visibility = Visibility.Collapsed;
            AppManager.DismissErrors();
        }

        private void ThemeLightRadio_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.AppTheme = AppTheme.Light;
            SettingsManager.Instance.Save();
            SettingsManager.Instance.Apply();
        }

        private void ThemeDarkRadio_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.AppTheme = AppTheme.Dark;
            SettingsManager.Instance.Save();
            SettingsManager.Instance.Apply();
        }

        private void ThemeUseSystemRadio_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.AppTheme = AppTheme.UseSystemSetting;
            SettingsManager.Instance.Save();
            SettingsManager.Instance.Apply();
        }

        private void LockDialogTimeout_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LockDialogTimeout.Text) ||
                LockDialogTimeout.Value < LockDialogTimeout.Minimum)
                LockDialogTimeout.Value = 3000;

            SettingsManager.Instance.ApplicationExitedWaitTime = (int)LockDialogTimeout.Value;
            SettingsManager.Instance.Save();
            SettingsManager.Instance.Apply();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LockDialogTimeout.Text) ||
                LockDialogTimeout.Value < LockDialogTimeout.Minimum)
                LockDialogTimeout.Value = 3000;

            SettingsManager.Instance.ApplicationExitedWaitTime = (int)LockDialogTimeout.Value;
            SettingsManager.Instance.Save();
            base.OnNavigatedFrom(e);
        }

        private void LanguagesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RestartButton.Visibility = Visibility.Visible;
            RestartRecommendedTextBlock.Visibility = Visibility.Visible;
            SettingsManager.Instance.CurrentLanguage = e.AddedItems.FirstOrDefault() as Language;
            Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
            Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
            SettingsManager.Instance.Save();
            SettingsManager.Instance.Apply();
        }

        private async void RestartButton_OnClick(object sender, RoutedEventArgs e)
        {
            await CoreApplication.RequestRestartAsync("");
        }
    }
}
