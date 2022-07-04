using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace craftersmine.AppLauncher.Core
{
    [Serializable]
    public class SettingsManager
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(SettingsManager), new XmlRootAttribute("Settings"));
        [XmlIgnore]
        public static SettingsManager Instance { get; set; }
        public static ElementTheme SystemTheme { get; set; }

        public int ApplicationExitedWaitTime { get; set; }
        public bool EnableSound { get; set; }
        public AppTheme AppTheme { get; set; }
        public string Language { get; set; }

        [XmlIgnore]
        public Language CurrentLanguage
        {
            get
            {
                return new Language(Language);
            }
            set
            {
                Language = value.LanguageTag;
            }
        }

        [XmlIgnore]
        public ElementTheme ApplicationTheme { get {
            switch (AppTheme)
            {
                    case AppTheme.UseSystemSetting:
                        var window = Window.Current.Content as FrameworkElement;
                        if (window is null)
                            return ElementTheme.Default;
                        return window.ActualTheme;
                    case AppTheme.Dark:
                        return ElementTheme.Dark;
                    case AppTheme.Light:
                        return ElementTheme.Light;
            }

            return ElementTheme.Default;
        } }

        public SettingsManager()
        {
            ApplicationExitedWaitTime = 3000;
            EnableSound = false;
            AppTheme = AppTheme.UseSystemSetting;
            Language = "en";
            Instance = this;
        }

        public void Save()
        {
            ApplicationData.Current.LocalSettings.Values["ApplicationExitWaitTime"] = ApplicationExitedWaitTime;
            ApplicationData.Current.LocalSettings.Values["EnableSound"] = EnableSound;
            ApplicationData.Current.LocalSettings.Values["AppTheme"] = (int)AppTheme;
            ApplicationData.Current.LocalSettings.Values["Language"] = Language;
        }

        public void Apply()
        {
            // Sound
            ElementSoundPlayer.State = SettingsManager.Instance.EnableSound
                ? ElementSoundPlayerState.On
                : ElementSoundPlayerState.Off;

            // Theme
            ApplyTheme();

            // Language
            ApplicationLanguages.PrimaryLanguageOverride = CurrentLanguage.LanguageTag;
        }

        private void ApplyTheme()
        {
            var window = Window.Current.Content as FrameworkElement;
            if (window is null)
                return;

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            Color fgColor = Colors.White;
            switch (AppTheme)
            {
                case AppTheme.Dark:
                    fgColor = Colors.White;
                    break;
                case AppTheme.Light:
                    fgColor = Colors.Black;
                    break;
                case AppTheme.UseSystemSetting:
                    switch (SystemTheme)
                    {
                        case ElementTheme.Light:
                            fgColor = Colors.Black;
                            break;
                        case ElementTheme.Dark:
                            fgColor = Colors.White;
                            break;
                    }
                    break;
            }

            titleBar.ButtonForegroundColor = fgColor;
            if (AppTheme != AppTheme.UseSystemSetting)
                window.RequestedTheme = ApplicationTheme;
            else
                window.RequestedTheme = SystemTheme;
        }

        public void Load()
        {
            try
            {
                ApplicationExitedWaitTime = (int) ApplicationData.Current.LocalSettings.Values["ApplicationExitWaitTime"];
                EnableSound = (bool) ApplicationData.Current.LocalSettings.Values["EnableSound"];
                AppTheme = (AppTheme) ((int)ApplicationData.Current.LocalSettings.Values["AppTheme"]);
                Language = (string) ApplicationData.Current.LocalSettings.Values["Language"];

            }
            catch (Exception e)
            {
                CreateDefault();
            }
            Apply();
        }

        private void CreateDefault()
        {
            ApplicationData.Current.LocalSettings.Values["ApplicationExitWaitTime"] = 3000;
            ApplicationData.Current.LocalSettings.Values["EnableSound"] = false;
            ApplicationData.Current.LocalSettings.Values["AppTheme"] = (int)AppTheme.UseSystemSetting;
            ApplicationData.Current.LocalSettings.Values["Language"] = "en-US";
            Load();
        }

        public static Language[] GetAvailableLanguages()
        {
            List<Language> languages = new List<Language>();

            foreach (var lang in ApplicationLanguages.ManifestLanguages)
            {
                languages.Add(new Language(lang));
            }

            return languages.ToArray();
        }
    }

    public enum AppTheme
    {
        UseSystemSetting = 0,
        Light = 1,
        Dark = 2,
    }
}
