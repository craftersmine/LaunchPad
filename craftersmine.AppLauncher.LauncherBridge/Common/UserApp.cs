using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace craftersmine.AppLauncher.LauncherBridgeClient.Common
{
    public class UserApp
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Uuid { get; set; }
        public string ExecutablePath { get; set; }
        public string LaunchArguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool RunAsAdmin { get; set; }

        public static UserApp DeserializeFromString(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserApp));
            using (TextReader tReader = new StringReader(data))
            {
                return serializer.Deserialize(tReader) as UserApp;
            }
        }

        public string SerializeToString()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserApp));
            using (StringWriter tWriter = new StringWriter())
            {
                serializer.Serialize(tWriter, this);
                return tWriter.ToString();
            }
        }
    }
}
