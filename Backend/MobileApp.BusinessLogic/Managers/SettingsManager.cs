using System;
using System.IO;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using NLog;

namespace MobileApp.BusinessLogic.Managers {
    public class SettingsManager : ISettingsManager {

        private ILogger Logger;

        private IConfigurationManager ConfigurationManager;

        private IFileStorage FileStorage;

        public SettingsManager(ILoggerService logger, IConfigurationManager configurationManager, IFileStorage fileStorage) {
            Logger = logger.GetLogger<SettingsManager>();
            ConfigurationManager = configurationManager;
            FileStorage = fileStorage;
        }

        public async Task<ApplicationSettingsDto> GetApplicationSettings() {
            Logger.Info("[GetApplicationSettings]Loading application settings.");
            throw new NotImplementedException();
        }

        public async Task UpdateCurrentSettings(Func<ApplicationSettingsDto, ApplicationSettingsDto> updateFunc) {
            UpdateSettings(updateFunc(await GetApplicationSettings()));
        }

        private async Task UpdateSettings(ApplicationSettingsDto newSettings) {
            Logger.Info("[UpdateSettings]Writing to application settings.");
            throw new NotImplementedException();
        }

        private async Task CreateDefaultSettingsByMissingFile() {
            string settingsFilePath = (await ConfigurationManager.GetConfig()).FileNames.SettingsFileName;
            if (!File.Exists(settingsFilePath)) {
                Logger.Info("[SettingsManager]Creating default settings file.");

                // create default settings file
                await UpdateSettings(ApplicationSettingsDto.GetStandardSettings());
            }
        }
    }
}
