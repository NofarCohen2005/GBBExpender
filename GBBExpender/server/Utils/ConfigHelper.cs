using System.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GbbExpender.Utils
{
    public static class ConfigHelper
    {
        private static string ConfigPath => Path.Combine(Directory.GetCurrentDirectory(), "App.config");

        public static Dictionary<string, string> GetAppSettings()
        {
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = ConfigPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            
            var settings = config.AppSettings.Settings;
            var dict = new Dictionary<string, string>();
            foreach (string key in settings.AllKeys)
            {
                dict[key] = settings[key].Value;
            }
            return dict;
        }

        public static void UpdateAppSettings(Dictionary<string, string> newSettings)
        {
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = ConfigPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            foreach (var (key, value) in newSettings)
            {
                if (config.AppSettings.Settings[key] != null)
                {
                    config.AppSettings.Settings[key].Value = value;
                }
                else
                {
                    config.AppSettings.Settings.Add(key, value);
                }
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
