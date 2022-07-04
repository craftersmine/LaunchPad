using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace craftersmine.AppLauncher.Core
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

        public static ProcessExitedInformation DeserializeFromString(string str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProcessExitedInformation));
            using (StringReader reader = new StringReader(str))
            {
                var obj = serializer.Deserialize(reader) as ProcessExitedInformation;
                return obj;
            }
        }
    }
}
