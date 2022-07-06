using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using craftersmine.AppLauncher.SteamGridDb;

namespace craftersmine.AppLauncher.Core
{
    public class LocalCoverStorage
    {
        private StorageFolder _localCoverStorageFolder;

        public ObservableCollection<SteamGridDbGridCover> LocalCovers { get; set; } =
            new ObservableCollection<SteamGridDbGridCover>();

        public static LocalCoverStorage Instance { get; set; }

        public LocalCoverStorage()
        {
            InitializeFolder();

            Instance = this;
        }

        private async void InitializeFolder()
        {
            _localCoverStorageFolder = await ApplicationData.Current.LocalFolder.TryGetItemAsync("localCoverStorage") as StorageFolder;
            if (_localCoverStorageFolder is null)
                _localCoverStorageFolder =
                    await ApplicationData.Current.LocalFolder.CreateFolderAsync("localCoverStorage");
        }

        public async Task<bool> DownloadCover(SteamGridDbGridCover cover)
        {
            try
            {
                string fileExt = Path.GetExtension(cover.FullImageUrl);
                var coverFile =
                    await _localCoverStorageFolder.TryGetItemAsync(cover.Id + fileExt);

                if (!(coverFile is null))
                    await coverFile.DeleteAsync();

                coverFile = await _localCoverStorageFolder.CreateFileAsync(cover.Id + fileExt);

                await cover.DownloadToAsync(coverFile.Path);
                cover.FullImageUrl = coverFile.Path;
                cover.ThumbnailImageUrl = coverFile.Path;
                cover.Locked = true;

                var existingCover = LocalCovers.FirstOrDefault(c => c.Id == cover.Id);
                int id = 0;
                if (!(existingCover is null))
                {
                    id = LocalCovers.IndexOf(existingCover);
                    LocalCovers.Remove(existingCover);
                }
                LocalCovers.Insert(id, cover);

                return await SaveCoversData();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveCoversData()
        {
            try
            {
                var localCoverDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("LocalCoverStorage.xml", CreationCollisionOption.ReplaceExisting);

                using (var applistStream = await localCoverDataFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<SteamGridDbGridCover>), new XmlRootAttribute("LocalCoverStorage"));
                    xmlSerializer.Serialize(applistStream.AsStream(), LocalCovers);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async void LoadCoversData()
        {
            try
            {
                var localCoverDataFile =
                    await ApplicationData.Current.LocalFolder.TryGetItemAsync("LocalCoverStorage.xml") as StorageFile;

                if (localCoverDataFile is null)
                    return;

                using (var stream = await localCoverDataFile.OpenStreamForReadAsync())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<SteamGridDbGridCover>), new XmlRootAttribute("LocalCoverStorage"));
                    LocalCovers = xmlSerializer.Deserialize(stream) as ObservableCollection<SteamGridDbGridCover>;
                }
            }
            catch (Exception e)
            {
                return;
            }
        }

        public async Task<bool> RemoveCoverById(int id)
        {
            try
            {
                var cover = LocalCovers.FirstOrDefault(c => c.Id == id);

                var fileName = Path.GetFileName(cover.FullImageUrl);
                var file = await _localCoverStorageFolder.TryGetItemAsync(fileName);
                if (!(file is null))
                    await file.DeleteAsync();
                LocalCovers.Remove(cover);
                await SaveCoversData();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
