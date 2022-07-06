using craftersmine.AppLauncher.Core;

using System;
using System.Collections.Generic;
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

namespace craftersmine.AppLauncher.Pages
{
    public sealed partial class UserAppEditor : Page
    {
        bool isCreatingApp = false;
        UserApp editingApp = null;

        public UserAppEditor()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is null)
                isCreatingApp = true;

            LoadPageData(e.Parameter as UserApp);

            base.OnNavigatedTo(e);
        }

        private void LoadPageData(UserApp app)
        {
            editingApp = app;

            if (isCreatingApp)
            {
                PageHeaderTextBlock.Text = ResourceManagers.StringsUserAppEditorResources.GetString("PageHeaderTextBlock_Add");
                editingApp = new UserApp();
                AppCoverImage.Source = editingApp.Image;
                editingApp.Name = ResourceManagers.StringsUserAppEditorResources.GetString("NewAppDefaultName");
                editingApp.ExecutablePath = "";
                editingApp.Description = "";
                editingApp.LaunchArguments = "";
                editingApp.WorkingDirectory = "";
                editingApp.RunAsAdmin = false;
                editingApp.Uuid = Guid.NewGuid().ToString();
                SaveButton.IsEnabled = false;
            }

            AppCoverImage.Source = editingApp.Image;
            AppNameTextBox.Text = editingApp.Name;
            AppExecutablePathTextBox.Text = editingApp.ExecutablePath;
            AppDescriptionTextBox.Text = editingApp.Description;
            AppWorkingDirectoryPathTextBox.Text = editingApp.WorkingDirectory;
            AppLaunchArgumentsTextBox.Text = editingApp.LaunchArguments;
            RunAsAdminCheckBox.IsChecked = editingApp.RunAsAdmin;
        }

        private async void editCoverButton_Click(object sender, RoutedEventArgs e)
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
                    var oldCover = await coversDir.GetFileAsync(Path.GetFileName(editingApp.ImagePath));
                    await oldCover.DeleteAsync();
                }
                catch (Exception)
                {

                }

                editingApp.Uuid = Guid.NewGuid().ToString();
                var cover = await coversDir.CreateFileAsync(editingApp.Uuid + selectedFile.FileType, CreationCollisionOption.ReplaceExisting);
                await selectedFile.CopyAndReplaceAsync(cover);

                editingApp.ImagePath = cover.Path;
                AppCoverImage.Source = editingApp.Image;
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
            editingApp = null;
            NavigateBack();
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButtonTextBlock.Visibility = Visibility.Collapsed;
            SavingProgressRing.Visibility = Visibility.Visible;

            editingApp.Description = AppDescriptionTextBox.Text;
            editingApp.Name = AppNameTextBox.Text;
            editingApp.LaunchArguments = AppLaunchArgumentsTextBox.Text;
            editingApp.RunAsAdmin = RunAsAdminCheckBox.IsChecked ?? false;

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
                editingApp.ExecutablePath = AppExecutablePathTextBox.Text;
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
                    editingApp.WorkingDirectory = AppWorkingDirectoryPathTextBox.Text;
                else
                {
                    SaveButtonTextBlock.Visibility = Visibility.Visible;
                    SavingProgressRing.Visibility = Visibility.Collapsed;
                    await ShowErrorDialog(true);
                    return;
                }
            }
            else
                editingApp.WorkingDirectory = "";

            if (!isCreatingApp)
            {
                var lastIndex = AppManager.Apps.IndexOf(editingApp);
                AppManager.Apps.Remove(editingApp);
                AppManager.Apps.Insert(lastIndex, editingApp);
            }
            else AppManager.Apps.Add(editingApp);

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
    }
}
