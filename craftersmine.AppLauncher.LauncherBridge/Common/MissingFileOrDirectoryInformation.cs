using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace craftersmine.AppLauncher.LauncherBridgeClient.Common
{
    public class MissingFileOrDirectoryInformation
    {
        public MissingEntityType Type { get; set; } = MissingEntityType.Unknown;
        public string AppUuid { get; set; }


        public override string ToString()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MissingFileOrDirectoryInformation));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }

    public enum MissingEntityType
    {
        Executable,
        WorkingDirectory,
        Unknown
    }
}
