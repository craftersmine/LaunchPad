using craftersmine.AppLauncher.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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
using Windows.UI.Xaml.Navigation;
using craftersmine.AppLauncher.Common;
using craftersmine.AppLauncher.SteamGridDb;
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;
using SplitButtonClickEventArgs = Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs;

namespace craftersmine.AppLauncher.Pages
{
    public sealed partial class UserAppEditor : Page
    {
        bool _isCreatingApp = false;
        UserApp _editingApp = null;
        private string _creatingAppExePath = "";

        public UserAppEditor()
        {
            this.InitializeComponent();
            SelectCoverFromLibraryButton.IsEnabled = false;
            if (LocalCoverStorage.Instance.LocalCovers.Count == 0)
            {
                NoCoversLabel.Visibility = Visibility.Visible;
                CoversLibrary.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoCoversLabel.Visibility = Visibility.Collapsed;
                CoversLibrary.Visibility = Visibility.Visible;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is null)
                _isCreatingApp = true;

            if (e.Parameter is UserApp)
            {
                LoadPageData(e.Parameter as UserApp);
                return;
            }

            if (e.Parameter is string)
            {
                _creatingAppExePath = e.Parameter as string;
                _isCreatingApp = true;
            }
            LoadPageData(null);

            base.OnNavigatedTo(e);
        }

        private void LoadPageData(UserApp app)
        {
            _editingApp = app;

            if (_isCreatingApp)
            {
                PageHeaderTextBlock.Text = ResourceManagers.StringsUserAppEditorResources.GetString("PageHeaderTextBlock_Add");
                _editingApp = new UserApp();
                AppCoverImage.Source = _editingApp.Image;
                _editingApp.Name = ResourceManagers.StringsUserAppEditorResources.GetString("NewAppDefaultName");
                _editingApp.ExecutablePath = _creatingAppExePath;
                _editingApp.Description = "";
                _editingApp.LaunchArguments = "";
                _editingApp.WorkingDirectory = "";
                _editingApp.RunAsAdmin = false;
                _editingApp.Uuid = Guid.NewGuid().ToString();
                SaveButton.IsEnabled = false;
            }

            AppCoverImage.Source = _editingApp.Image;
            AppNameTextBox.Text = _editingApp.Name;
            if (!string.IsNullOrWhiteSpace(_creatingAppExePath))
                AppNameTextBox.Text = Path.GetFileNameWithoutExtension(_creatingAppExePath);
            AppExecutablePathTextBox.Text = _editingApp.ExecutablePath;
            AppDescriptionTextBox.Text = _editingApp.Description;
            AppWorkingDirectoryPathTextBox.Text = _editingApp.WorkingDirectory;
            AppLaunchArgumentsTextBox.Text = _editingApp.LaunchArguments;
            RunAsAdminCheckBox.IsChecked = _editingApp.RunAsAdmin;

            if (!string.IsNullOrWhiteSpace(AppNameTextBox.Text) &&
                !string.IsNullOrWhiteSpace(AppExecutablePathTextBox.Text))
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async void SelectFromFileClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker coverFilePicker = new FileOpenPicker();
            coverFilePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            coverFilePicker.ViewMode = PickerViewMode.Thumbnail;
            coverFilePicker.FileTypeFilter.Add(".jpeg");
            coverFilePicker.FileTypeFilter.Add(".jpg");
            coverFilePicker.FileTypeFilter.Add(".png");
            coverFilePicker.FileTypeFilter.Add(".gif");
            coverFilePicker.FileTypeFilter.Add(".bmp");
            coverFilePicker.CommitButtonText = ResourceManagers.StringsUserAppEditorResources.GetString("ImagePicker_SelectImageButton");
            StorageFile image = await coverFilePicker.PickSingleFileAsync();

            ApplyCover(image);
        }

        public async void ApplyCover(StorageFile selectedFile)
        {
            if (selectedFile is null)
                return;

            if (selectedFile.ContentType.Contains("image"))
            {
                try
                {
                    if ((await ApplicationData.Current.LocalFolder.GetFolderAsync("covers")) is null)
                        await ApplicationData.Current.LocalFolder.CreateFolderAsync("covers");
                }
                catch
                {
                    await ApplicationData.Current.LocalFolder.CreateFolderAsync("covers");
                }
                var coversDir = await ApplicationData.Current.LocalFolder.GetFolderAsync("covers");
                try
                {
                    var oldCover = await coversDir.GetFileAsync(Path.GetFileName(_editingApp.ImagePath));
                    await oldCover.DeleteAsync();
                }
                catch (Exception)
                {

                }

                _editingApp.Uuid = Guid.NewGuid().ToString();
                var cover = await coversDir.CreateFileAsync(_editingApp.Uuid + selectedFile.FileType, CreationCollisionOption.ReplaceExisting);
                await selectedFile.CopyAndReplaceAsync(cover);

                _editingApp.ImagePath = cover.Path;
                AppCoverImage.Source = _editingApp.Image;
            }
        }

        private void appNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AppNameTextBox.Text.Length > 0 && AppExecutablePathTextBox.Text.Length > 0)
                SaveButton.IsEnabled = true;
            else SaveButton.IsEnabled = false;
        }

        private void appExecutablePathTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AppNameTextBox.Text.Length > 0 && AppExecutablePathTextBox.Text.Length > 0)
                SaveButton.IsEnabled = true;
            else SaveButton.IsEnabled = false;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            _editingApp = null;
            NavigateBack();
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButtonTextBlock.Visibility = Visibility.Collapsed;
            SavingProgressRing.Visibility = Visibility.Visible;

            _editingApp.Description = AppDescriptionTextBox.Text;
            _editingApp.Name = AppNameTextBox.Text;
            _editingApp.LaunchArguments = AppLaunchArgumentsTextBox.Text;
            _editingApp.RunAsAdmin = RunAsAdminCheckBox.IsChecked ?? false;

            FileExistsData filesData;
            if (AppWorkingDirectoryPathTextBox.Text.Length > 0)
                filesData = await AppManager.CheckFileAndDirsExists(new FileDirData()
                    {
                        Exists = false,
                        IsDirectory = false,
                        Path = AppExecutablePathTextBox.Text
                    },
                    new FileDirData()
                        {Exists = false, IsDirectory = true, Path = AppWorkingDirectoryPathTextBox.Text});
            else
                filesData = await AppManager.CheckFileAndDirsExists(new FileDirData()
                    {
                        Exists = false,
                        IsDirectory = false,
                        Path = AppExecutablePathTextBox.Text
                    });

            if (filesData.FilesAndDirs.First(f => f.Path == AppExecutablePathTextBox.Text).Exists)
                _editingApp.ExecutablePath = AppExecutablePathTextBox.Text;
            else
            {
                SaveButtonTextBlock.Visibility = Visibility.Visible;
                SavingProgressRing.Visibility = Visibility.Collapsed;
                await ShowErrorDialog(false);
                return;
            }

            if (AppWorkingDirectoryPathTextBox.Text.Length > 0)
            {
                if (filesData.FilesAndDirs.First(d => d.Path == AppWorkingDirectoryPathTextBox.Text).Exists)
                    _editingApp.WorkingDirectory = AppWorkingDirectoryPathTextBox.Text;
                else
                {
                    SaveButtonTextBlock.Visibility = Visibility.Visible;
                    SavingProgressRing.Visibility = Visibility.Collapsed;
                    await ShowErrorDialog(true);
                    return;
                }
            }
            else
                _editingApp.WorkingDirectory = "";

            if (!_isCreatingApp)
            {
                var lastIndex = AppManager.Apps.IndexOf(_editingApp);
                AppManager.Apps.Remove(_editingApp);
                AppManager.Apps.Insert(lastIndex, _editingApp);
            }
            else AppManager.Apps.Add(_editingApp);

            var isSaved = await AppManager.RequestListSave();

            if (!isSaved)
            {
                ContentDialog errorMessage = new ContentDialog();
                errorMessage.CloseButtonText = ResourceManagers.StringsCommonResources.GetString("Ok");
                errorMessage.Content = ResourceManagers.StringsUserAppEditorResources.GetString("UnableToSaveApp_Content");
                errorMessage.Title = ResourceManagers.StringsUserAppEditorResources.GetString("UnableToSaveApp_Title");
                errorMessage.CloseButtonClick += ErrorMessage_CloseButtonClick;
                errorMessage.DefaultButton = ContentDialogButton.Close;
                await errorMessage.ShowAsync();
                return;
            }

            SaveButtonTextBlock.Visibility = Visibility.Visible;
            SavingProgressRing.Visibility = Visibility.Collapsed;
            NavigateBack();
        }

        private async Task ShowErrorDialog(bool forWorkingDirectory)
        {
            ContentDialog errorMessage = new ContentDialog();
            errorMessage.CloseButtonText = ResourceManagers.StringsCommonResources.GetString("Ok");
            errorMessage.Content = ResourceManagers.StringsUserAppEditorResources.GetString("UnableToFindExecutableDlg_Content");
            if (forWorkingDirectory)
                errorMessage.Content = ResourceManagers.StringsUserAppEditorResources.GetString("UnableToFindDirectoryDlg_Content");
            errorMessage.Title = ResourceManagers.StringsUserAppEditorResources.GetString("UnableToFindExecutableOrDirectoryDlg_Title");
            errorMessage.CloseButtonClick += ErrorMessage_CloseButtonClick;
            errorMessage.DefaultButton = ContentDialogButton.Close;
            await errorMessage.ShowAsync();
        }

        private static void ErrorMessage_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }

        private void NavigateBack()
        {
            var frame = ((Frame)Parent);
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            frame.Navigate(typeof(UserAppsPage));
            navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed;
            navView.IsBackEnabled = false;
        }

        private async void browseForExecutableButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".exe");
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            fileOpenPicker.CommitButtonText = ResourceManagers.StringsUserAppEditorResources.GetString("ExecutablePicker_SelectAppButton");
            var executableFile = await fileOpenPicker.PickSingleFileAsync();
            if (executableFile is null)
                return;

            AppExecutablePathTextBox.Text = executableFile.Path;
        }

        private async void browseForWorkingDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.CommitButtonText = ResourceManagers.StringsUserAppEditorResources.GetString("WorkingDirectoryPicker_SelectDirectoryButton");
            folderPicker.ViewMode = PickerViewMode.List;
            var pickedFolder = await folderPicker.PickSingleFolderAsync();
            if (pickedFolder is null)
                return;

            AppWorkingDirectoryPathTextBox.Text = pickedFolder.Path;
        }

        private async void ApplySelectedCoverButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedCover = CoversLibrary.SelectedItem as SteamGridDbGridCover;
            ApplyCover(await StorageFile.GetFileFromPathAsync(selectedCover.FullImageUrl));
            SelectCoverButton.Flyout.Hide();
        }

        private void FindCoversClick(object sender, RoutedEventArgs e)
        {
            var frame = ((Frame)Parent);
            var navView = ((Microsoft.UI.Xaml.Controls.NavigationView)frame.Parent);
            frame.Navigate(typeof(SteamGridDbSearchPage));
            navView.SelectedItem = navView.MenuItems.FirstOrDefault(i =>
                (i as Microsoft.UI.Xaml.Controls.NavigationViewItem).Name.ToLower().Contains("steamgriddb"));
            navView.IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed;
            navView.IsBackEnabled = false;
        }

        private void CoversLibrary_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectCoverFromLibraryButton.IsEnabled = !(CoversLibrary.SelectedItem is null);
        }
    }
}
