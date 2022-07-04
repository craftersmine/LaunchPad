using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace craftersmine.AppLauncher.Core
{
    public class VersionManager
    {
        public static string Version => string.Format("{0}.{1}.{2}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build);
    }
}
