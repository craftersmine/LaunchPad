using craftersmine.AppLauncher.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.System;
using tiesky.com;

namespace craftersmine.AppLauncher.Common
{
    public class IPCServer
    {
        private SharmIpc ipc;

        public event EventHandler<RemoteCallEventArgs> RemoteCall;

        public IPCServer(string ipcPipeName)
        {
            ipc = new SharmIpc(ipcPipeName, _remoteCall);
        }

        private Tuple<bool, byte[]> _remoteCall(byte[] data)
        {
            string str = Encoding.Default.GetString(data);
            RemoteCall?.Invoke(this, new RemoteCallEventArgs() { Data = str });

            return new Tuple<bool, byte[]>(true, new byte[] { });
        }
        
        public async void SendLaunchApp(UserApp app)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserApp));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, app);
                var response = await ipc.RemoteRequestAsync(Encoding.Default.GetBytes("APPLAUNCH::" + sw.ToString()));
                if (response.Item1)
                {
                    if (response.Item2 is null)
                        return;

                    string[] data = Encoding.Default.GetString(response.Item2).Split("::", StringSplitOptions.RemoveEmptyEntries);

                    string header = data[0];
                    string payload = "";
                    if (data.Length > 1)
                        payload = data[1];

                    switch (header)
                    {
                        case "APPLAUNCHED":
                            var bridgeApp = UserApp.DeserializeFromString(payload);
                            AppManager.RaiseApplicationEvent(bridgeApp.Uuid, ApplicationEventType.Launched);
                            break;
                    }
                }
            }
        }

        public async Task<FileExistsData> SendCheckFilesAndDirsExists(FileDirData[] filesAndDirs)
        {
            FileExistsData fileExistsData = new FileExistsData();
            fileExistsData.FilesAndDirs.AddRange(filesAndDirs);
            var response = await ipc.RemoteRequestAsync(Encoding.Default.GetBytes("CHECKFILESEXISTANCE::" + fileExistsData.SerializeToString()));
            if (response.Item1)
            {
                var data = Encoding.Default.GetString(response.Item2).Split("::", StringSplitOptions.RemoveEmptyEntries);
                string header = data[0];
                string payload = "";
                if (data.Length > 1)
                    payload = data[1];

                if (header != "FILESEXISTANCEDATA")
                    return null;

                return FileExistsData.DeserializeFromString(payload);
            }

            return null;
        }

        public void SendBridgeExit()
        {
            ipc.RemoteRequestWithoutResponse(Encoding.Default.GetBytes("BRIDGEEXIT"));
        }

        public async Task<bool> SendLaunchedAppToClose(UserApp launchedApp)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserApp));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, launchedApp);
                var response = await ipc.RemoteRequestAsync(Encoding.Default.GetBytes("CLOSEAPP::" + writer.ToString()));
                if (response.Item1)
                {
                    if (response.Item2 is null)
                        return true;

                    var responseData = Encoding.Default.GetString(response.Item2)
                        .Split("::", StringSplitOptions.RemoveEmptyEntries);

                    string header = responseData[0];
                    string payload = "";
                    if (responseData.Length > 1)
                        payload = responseData[1];

                    switch (header)
                    {
                        case "APPEXITED":
                            return true;
                    }
                }
            }

            return false;
        }

        public void SendLaunchedAppToTerminate(UserApp launchedApp)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserApp));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, launchedApp);
                var response = ipc.RemoteRequestWithoutResponse(Encoding.Default.GetBytes("TERMINATEAPP::" + writer.ToString()));
            }
        }
    }

    public class RemoteCallEventArgs : EventArgs
    { 
        public string Data { get; set; }
    }
}
