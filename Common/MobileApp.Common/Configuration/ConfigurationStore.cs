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
    }
}
