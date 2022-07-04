using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace craftersmine.AppLauncher.Core
{
    [Serializable]
    public class UserApp
    {
        private ImageSource image;

        [XmlAttribute]
        public string Name { get; set; }
        public string ImagePath { get; set; }
        [XmlIgnore]
        public ImageSource Image { 
            get 
            {
                if (string.IsNullOrWhiteSpace(ImagePath))
                {
                    image = new BitmapImage(new Uri("ms-appx:///Assets/Images/NoCover.png"));
                    return image;
                }
                image = new BitmapImage(new Uri(ImagePath));
                return image;
            } 
        }
        public string Description { get; set; }
        [XmlAttribute]
        public string Uuid { get; set; }
        public string ExecutablePath { get; set; }
        public string LaunchArguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool RunAsAdmin { get; set; }
        public bool IsFavorited { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public void UnloadImage()
        {
            image = null;
            ImagePath = null;
        }

        public static UserApp DeserializeFromString(string str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserApp));
            using (StringReader reader = new StringReader(str))
            {
                var obj = serializer.Deserialize(reader) as UserApp;
                return obj;
            }
        }
    }
}
