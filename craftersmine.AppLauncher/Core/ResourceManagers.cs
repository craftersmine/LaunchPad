using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace craftersmine.AppLauncher.Core
{
    public class ResourceManagers
    {
        public static ResourceLoader StringsCommonResources => ResourceLoader.GetForCurrentView();
        public static ResourceLoader StringsSettingsResources => ResourceLoader.GetForCurrentView("Settings");
        public static ResourceLoader StringsUserAppsResources => ResourceLoader.GetForCurrentView("UserApps");
        public static ResourceLoader StringsUserAppInfoResources => ResourceLoader.GetForCurrentView("UserAppInfoPage");
        public static ResourceLoader StringsUserAppEditorResources => ResourceLoader.GetForCurrentView("UserAppEditor");
    }
}
