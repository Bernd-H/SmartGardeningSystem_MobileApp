using System.Threading.Tasks;
using MobileApp.Common.Models.Entities.Configuration;
using MobileApp.Common.Specifications.Configuration;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using Newtonsoft.Json;

namespace MobileApp.BusinessLogic.Managers {
    public class ConfigurationManager : IConfigurationManager {

        private IConfiguration config;

        private IFileStorage FileStorage;

        public ConfigurationManager(IFileStorage fileStorage) {
            FileStorage = fileStorage;
        }

        public async Task<IConfiguration> GetConfig() {
            if (config == null) {
                // load and deserialize config file
                var configurationFile = await FileStorage.ReadAsString("config.json");

                var configuration = JsonConvert.DeserializeObject<Configuration>(configurationFile);

                config = configuration;
            }

            return config;
        }
    }
}
