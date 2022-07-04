using craftersmine.AppLauncher.LauncherBridgeClient.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace craftersmine.AppLauncher.LauncherBridgeClient
{
    internal static class Program
    {
        public const string LauncherBridgePipeId = @"AppContainerNamedObjects\S-1-15-2-2286578467-3750320569-2933599537-154834458-1712162063-163972942-256443250\AppLauncherBridgePipeId_{1699af15-c7a1-4bfd-97ce-c98e1c00d827}";
        public static IPCClient IPCClient { get; private set; }
        public static ApplicationContext Context { get; private set; }
        public static ProcessManager ProcessManager { get; set; }
        [STAThread]
        static void Main(string[] args)
        {
            IPCClient = new IPCClient(LauncherBridgePipeId);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Context = new ApplicationContext();
            Application.Run(Context);
        }

        public static void Exit()
        {
            if (!(ProcessManager is null) && ProcessManager.Exited)
            {
                if (!ProcessManager.Exited)
                    ProcessManager.Close();
                Environment.Exit(0);
            }
            else Environment.Exit(0);
        }

        public static void DelayedExit()
        {
            Timer tmr = new Timer(o => Exit(), null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(-1));
        }

        public static void BridgeExit()
        {
            Application.Exit();
        }
    }
}
