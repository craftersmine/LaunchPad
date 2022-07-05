using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace craftersmine.AppLauncher.LauncherBridgeClient.Common
{
    public sealed class ProcessManager
    {
        private Process currentProcess;

        public UserApp CurrentApp { get; set; }
        public int ExitCode
        {
            get {
                try
                {
                    return currentProcess != null ? currentProcess.ExitCode : 0;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }
        }
        public bool Exited { get {
            try
            {
                return currentProcess != null ? currentProcess.HasExited : true;
            }
            catch (Exception e)
            {
                return true;
            }
        } }

        public ProcessManager(UserApp app)
        {
            CurrentApp = app;

            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.FileName = CurrentApp.ExecutablePath;

            if (!DetectExecutable())
            {
                Program.IPCClient.SendNoExecutableOrWorkingDirectoryFound(CurrentApp.Uuid, false);
                Program.Exit();
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentApp.WorkingDirectory))
                CurrentApp.WorkingDirectory = Path.GetDirectoryName(CurrentApp.ExecutablePath);

            if (!DetectWorkingDir())
            {
                Program.IPCClient.SendNoExecutableOrWorkingDirectoryFound(CurrentApp.Uuid, true);
                Program.Exit();
                return;
            }

            processStartInfo.Arguments = CurrentApp.LaunchArguments;

            processStartInfo.WorkingDirectory = CurrentApp.WorkingDirectory ?? string.Empty;

            if (app.RunAsAdmin)
                processStartInfo.Verb = "runas";

            currentProcess = new Process();
            currentProcess.StartInfo = processStartInfo;
            currentProcess.EnableRaisingEvents = true;
            currentProcess.Exited += CurrentProcess_Exited;
        }

        private void CurrentProcess_Exited(object sender, EventArgs e)
        {
            Program.IPCClient.SendProcessExited(CurrentApp.Uuid, ExitCode);
            Program.Exit();
        }

        private bool DetectExecutable()
        {
            try
            {
                return File.Exists(CurrentApp.ExecutablePath);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool DetectWorkingDir()
        {
            try
            {
                return Directory.Exists(CurrentApp.WorkingDirectory);
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public void Run()
        {
            try
            {
                currentProcess.Start();
            }
            catch (Exception e)
            {
                Program.IPCClient.SendProcessExited(CurrentApp.Uuid, ExitCode);
                Program.Exit();
            }
        }

        public void Close()
        {
            try
            {
                currentProcess.CloseMainWindow();
            }
            catch (Exception e)
            {
                Program.IPCClient.SendProcessExited(CurrentApp.Uuid, ExitCode);
                Program.Exit();
            }
        }

        public void Kill()
        {
            try
            {
                currentProcess.Kill();
            }
            catch (Exception e)
            {
                Program.IPCClient.SendProcessExited(CurrentApp.Uuid, ExitCode);
                Program.Exit();
            }
        }
    }
}
