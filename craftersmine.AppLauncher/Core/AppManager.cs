using craftersmine.AppLauncher.Common;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace craftersmine.AppLauncher.Core
{
    public class AppManager
    {
        private static UserApp launchedApp;
        private static IPCServer ipcServer = new IPCServer(LauncherBridgePipeId);
        private static DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public const string LauncherBridgePipeId = @"AppLauncherBridgePipeId_{1699af15-c7a1-4bfd-97ce-c98e1c00d827}";

        static AppManager()
        {
            ipcServer.RemoteCall += IpcServer_RemoteCall;
        }

        private static void IpcServer_RemoteCall(object sender, RemoteCallEventArgs e)
        {
            string[] data = e.Data.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

            System.Diagnostics.Debug.WriteLine(e.Data);

            string header = data[0];
            string payload = "";
            if (data.Length > 1)
                payload = data[1];

            switch (header)
            {
                case "APPEXITED":
                    var procExitInfo = ProcessExitedInformation.DeserializeFromString(payload);
                    ApplicationEvent?.Invoke(null, new ApplicationEventArgs() { ApplicationEventType = ApplicationEventType.Exited, AppUuid = procExitInfo.Uuid, ExitCode = procExitInfo.ExitCode });
                    break;
                case "APPLAUNCHED":
                    var bridgeApp = UserApp.DeserializeFromString(payload);
                    ApplicationEvent?.Invoke(null, new ApplicationEventArgs() { ApplicationEventType = ApplicationEventType.Launched, AppUuid = bridgeApp.Uuid, ExitCode = 0 });
                    break;
                case "MISSINGFILES":
                    var missingFilesInfo = MissingFileOrDirectoryInformation.DeserializeFromString(payload);
                    if (missingFilesInfo.Type == MissingEntityType.Executable)
                        ApplicationEvent?.Invoke(null, new ApplicationEventArgs() {ApplicationEventType = ApplicationEventType.MissingExecutable, AppUuid = missingFilesInfo.AppUuid, ExitCode = 0});
                    else if (missingFilesInfo.Type == MissingEntityType.WorkingDirectory)
                        ApplicationEvent?.Invoke(null, new ApplicationEventArgs() { ApplicationEventType = ApplicationEventType.MissingWorkingDirectory, AppUuid = missingFilesInfo.AppUuid, ExitCode = 0 });
                    break;
            }
        }

        public static void RaiseApplicationEvent(string uuid, ApplicationEventType eventType, int exitCode = 0)
        {
            ApplicationEvent?.Invoke(null, new ApplicationEventArgs() { ApplicationEventType = eventType, AppUuid = uuid, ExitCode = exitCode });
        }

        public static ObservableCollection<UserApp> Apps { get; set; } = new ObservableCollection<UserApp>();
        public static bool IsFailedToLoadList { get; private set; }

        public static UserApp GetApplicationByUuid(string appUuid)
        {
            return Apps.First(a => a.Uuid == appUuid);
        }

        public static bool HasBrokenAppList { get; private set; }

        public static event EventHandler<ApplicationEventArgs> ApplicationEvent;

        public static async Task<bool> RequestListSave()
        {
            try
            {
                var applistFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("AppList.xml", CreationCollisionOption.ReplaceExisting);

                using (var applistStream = await applistFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<UserApp>), new XmlRootAttribute("AppList"));
                    xmlSerializer.Serialize(applistStream.AsStream(), Apps);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async void LoadAppList()
        {
            var brokenApplistFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("AppList.broken.xml") as StorageFile;
            if (!(brokenApplistFile is null))
                HasBrokenAppList = true;

            var applistFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("AppList.xml") as StorageFile;
            if (applistFile is null)
                return;

            try
            {
                using (var stream = await applistFile.OpenStreamForReadAsync())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<UserApp>), new XmlRootAttribute("AppList"));
                    Apps = xmlSerializer.Deserialize(stream) as ObservableCollection<UserApp>;
                }
            }
            catch (Exception)
            {
                await applistFile.RenameAsync("AppList.broken.xml");
                IsFailedToLoadList = true;
            }
        }

        public static void DismissErrors()
        {
            IsFailedToLoadList = false;
            HasBrokenAppList = false;
        }

        public static async void LaunchApp(UserApp app)
        {
            launchedApp = app;
            ApplicationEvent?.Invoke(null, new ApplicationEventArgs() { ApplicationEventType = ApplicationEventType.Launching, AppUuid = launchedApp.Uuid, ExitCode = 0 });
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("InitAppBridge");

            ipcServer.SendLaunchApp(launchedApp);
        }

        public static async Task<FileExistsData> CheckFileAndDirsExists(params FileDirData[] filesAndDirs)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            return await ipcServer.SendCheckFilesAndDirsExists(filesAndDirs);
        }

        private static void DispatcherTimer_Tick(object sender, object e)
        {
            ApplicationEvent?.Invoke(null, new ApplicationEventArgs()
            {
                ApplicationEventType = ApplicationEventType.LaunchTimedOut,
                AppUuid = launchedApp.Uuid,
                ExitCode = 0
            });
            dispatcherTimer.Stop();
        }

        public static void SendBridgeExit()
        {
            ipcServer.SendBridgeExit();
        }

        public static async void SendLaunchedAppClose()
        {
            var result = await ipcServer.SendLaunchedAppToClose(launchedApp);
            if (result)
                launchedApp = null;
        }

        public static void SendLaunchedAppTerminate()
        {
            ipcServer.SendLaunchedAppToTerminate(launchedApp);
            launchedApp = null;
        }
    }

    public class ApplicationEventArgs : EventArgs
    {
        public string AppUuid { get; set; }
        public ApplicationEventType ApplicationEventType { get; set; }
        public int ExitCode { get; set; }
    }

    public enum ApplicationEventType
    {
        Launching,
        Launched,
        Exited,
        LaunchTimedOut,
        MissingExecutable,
        MissingWorkingDirectory
    }
}
