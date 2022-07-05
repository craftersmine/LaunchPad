using System.IO;
using System.Xml.Serialization;

namespace craftersmine.AppLauncher.Core
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

        public static MissingFileOrDirectoryInformation DeserializeFromString(string str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MissingFileOrDirectoryInformation));
            using (StringReader reader = new StringReader(str))
            {
                var obj = serializer.Deserialize(reader) as MissingFileOrDirectoryInformation;
                return obj;
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
