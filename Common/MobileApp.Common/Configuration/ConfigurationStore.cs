using MobileApp.Common.Specifications.Configuration;
using Newtonsoft.Json;

namespace MobileApp.Common.Configuration {
    public static class ConfigurationStore {

        /// <summary>
        /// This file is in the Assets folder in MobileApp.Android
        /// </summary>
        public static readonly string ConfigFileName = "config.json";

        /// <summary>
        /// Gets set by MobileApp.Android and MobileApp.iOS when the app gets started.
        /// </summary>
        public static string ConfigurationContent;

        private static IConfiguration config = null;

        public static IConfiguration GetConfig() {
            if (config == null) {
                // load and deserialize config file
                //IFolder rootFolder = FileSystem.Current.LocalStorage; 
                //var path = rootFolder.Path;
                //var configurationFile = await FileStorage.ReadAsString(configFileName);

                var configuration = JsonConvert.DeserializeObject<MobileApp.Common.Models.Entities.Configuration.Configuration>(ConfigurationContent);
                config = configuration;
            }

            return config;
        }

        /// <summary>
        /// Used when loading the configuration file failed.
        /// </summary>
        public static void SetConfigToStandardValues() {
            #region Serialized standard configurationString
            string jsonString = "{\r\n  \"FileNames\": {\r\n    \"SettingsFileName\": \"settings.json\"\r\n  },\r\n  \"ConnectionSettings\": {\r\n    \"LocalPeerDiscovery_port\": 6771,\r\n    \"LocalPeerDiscovery_multicastIP\": \"239.192.152.143\",\r\n\r\n    \"API_Port\": 5001,\r\n    \"API_URL_Login\": \"https://{0}:{1}/api/Login/\",\r\n    \"API_URL_Modules\": \"https://{0}:{1}/api/Modules/\",\r\n\r\n    \"ConfigurationWiFi_ServerIP\": \"10.0.2.2\",\r\n    \"ConfigurationWiFi_KeyExchangeListenPort\": \"52143\"\r\n  }\r\n}";
            #endregion
            config = JsonConvert.DeserializeObject<MobileApp.Common.Models.Entities.Configuration.Configuration>(jsonString);
        }
    }
}
