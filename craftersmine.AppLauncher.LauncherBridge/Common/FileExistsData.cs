using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace craftersmine.AppLauncher.Common
{
    public class FileExistsData
    {
        public List<FileDirData> FilesAndDirs = new List<FileDirData>();

        public static FileExistsData DeserializeFromString(string str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FileExistsData));
            using (StringReader reader = new StringReader(str))
            {
                return serializer.Deserialize(reader) as FileExistsData;
            }
        }

        public string SerializeToString()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FileExistsData));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }

    public class FileDirData
    {
        public string Path { get; set; }
        public bool Exists { get; set; }
        public bool IsDirectory { get; set; }
    }
}
