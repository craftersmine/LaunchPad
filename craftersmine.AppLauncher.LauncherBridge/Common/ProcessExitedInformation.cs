using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace craftersmine.AppLauncher.LauncherBridgeClient.Common
{
    public class ProcessExitedInformation
    {
        public string Uuid { get; set; }
        public int ExitCode { get; set; }

        public ProcessExitedInformation() { }
             
        public ProcessExitedInformation(string uuid, int exitCode)
        {
            Uuid = uuid;
            ExitCode = exitCode;
        }

        public override string ToString()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProcessExitedInformation));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }
}
