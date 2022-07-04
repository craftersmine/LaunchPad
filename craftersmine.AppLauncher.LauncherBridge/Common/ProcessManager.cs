using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            processStartInfo.Arguments = CurrentApp.LaunchArguments;
            processStartInfo.FileName = CurrentApp.ExecutablePath;
            processStartInfo.WorkingDirectory = CurrentApp.WorkingDirectory;
            processStartInfo.UseShellExecute = true;

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
            // TODO: Implement executable location detection
            return true;
        }

        private bool DetectWorkingDir()
        {
            // TODO: Implement working directory location detection
            return true;

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
                currentProcess.Close();
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
