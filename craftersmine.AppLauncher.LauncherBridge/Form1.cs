using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace craftersmine.AppLauncher.LauncherBridgeClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Program.IPCClient.RemoteCall += IPCClient_RemoteCall;
        }

        private void IPCClient_RemoteCall(object sender, Common.RemoteCallEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
