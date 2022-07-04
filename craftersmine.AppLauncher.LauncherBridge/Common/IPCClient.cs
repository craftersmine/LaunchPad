using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using craftersmine.AppLauncher.Common;
using tiesky.com;

namespace craftersmine.AppLauncher.LauncherBridgeClient.Common
{
    public class IPCClient
    {
        private SharmIpc ipc;

        public event EventHandler<RemoteCallEventArgs> RemoteCall;

        public IPCClient(string ipcPipeName)
        {
            ipc = new SharmIpc(ipcPipeName, _remoteCall);
        }

        private Tuple<bool, byte[]> _remoteCall(byte[] data)
        {
            string[] dataStr = Encoding.Default.GetString(data).Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine(Encoding.Default.GetString(data));
            RemoteCall?.Invoke(this, new RemoteCallEventArgs() { Data = Encoding.Default.GetString(data) });
            string header = dataStr[0];
            string payload = "";
            if (dataStr.Length > 1)
                payload = dataStr[1];

            switch (header)
            {
                case "APPLAUNCH":
                    UserApp app = UserApp.DeserializeFromString(payload);
                    Program.ProcessManager = new ProcessManager(app);
                    Program.ProcessManager.Run();
                    return new Tuple<bool, byte[]>(true, Encoding.Default.GetBytes("APPLAUNCHED::" + app.SerializeToString()));
                case "CHECKFILESEXISTANCE":
                    FileExistsData fileData = FileExistsData.DeserializeFromString(payload);
                    foreach (var fileOrDir in fileData.FilesAndDirs)
                    {
                        if (fileOrDir.IsDirectory)
                            fileOrDir.Exists = Directory.Exists(fileOrDir.Path);
                        else fileOrDir.Exists = File.Exists(fileOrDir.Path);
                    }
                    Program.DelayedExit();
                    return new Tuple<bool, byte[]>(true, Encoding.Default.GetBytes("FILESEXISTANCEDATA::" + fileData.SerializeToString()));
                case "CLOSEAPP":
                    Program.ProcessManager.Close();
                    return new Tuple<bool, byte[]>(true, null);
                case "TERMINATEAPP":
                    Program.ProcessManager.Kill();
                    return new Tuple<bool, byte[]>(true, null);
            }

            return new Tuple<bool, byte[]>(false, null);
        }

        public void SendProcessExited(string appUuid, int exitCode)
        {
            string procExitInfo = new ProcessExitedInformation(appUuid, exitCode).ToString();
            ipc.RemoteRequestWithoutResponse(Encoding.Default.GetBytes("APPEXITED::" + procExitInfo));
        }
    }

    public class RemoteCallEventArgs : EventArgs
    {
        public string Data { get; set; }
    }
}
