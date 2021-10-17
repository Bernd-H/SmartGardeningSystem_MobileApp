using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.BusinessLogic.Managers {
    public class SettingsManager : ISettingsManager {

        private ILogger Logger;

        private IFileStorage FileStorage;

        private string settingsFilePath;

        private static SemaphoreSlim LOCKER = new SemaphoreSlim(1);

        public SettingsManager(ILoggerService logger, IFileStorage fileStorage) {
            Logger = logger.GetLogger<SettingsManager>();
            FileStorage = fileStorage;
        }

        public async Task<ApplicationSettingsDto> GetApplicationSettings() {
            await LOCKER.WaitAsync();

            Logger.Info("[GetApplicationSettings]Loading application settings.");
            SetFilePathIfEmpty();
            await CreateDefaultSettingsByMissingFile();

            var settingsRaw = FileStorage.ReadAsString(settingsFilePath).Result;
            var settings = JsonConvert.DeserializeObject<ApplicationSettingsDto>(settingsRaw);

            LOCKER.Release();
            return settings;
        }

        public async Task UpdateCurrentSettings(Func<ApplicationSettingsDto, ApplicationSettingsDto> updateFunc) {
            await UpdateSettings(updateFunc(await GetApplicationSettings()));
        }

        private async Task UpdateSettings(ApplicationSettingsDto newSettings) {
            await LOCKER.WaitAsync();

            Logger.Info("[UpdateSettings]Writing to application settings.");

            var jsonSettings = JsonConvert.SerializeObject(newSettings);
            await FileStorage.WriteAllText(settingsFilePath, jsonSettings);

            LOCKER.Release();
        }

        private async Task CreateDefaultSettingsByMissingFile() {
            if (!File.Exists(settingsFilePath)) {
                Logger.Info("[SettingsManager]Creating default settings file.");

                // create default settings file
                await UpdateSettings(ApplicationSettingsDto.GetStandardSettings());
            }
        }

        private void SetFilePathIfEmpty() {
            if (string.IsNullOrEmpty(settingsFilePath)) {
                var configuration = ConfigurationStore.GetConfig();
                settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), configuration.FileNames.SettingsFileName);
                //settingsFilePath = configuration.FileNames.SettingsFileName;
            }
        }
    }
}
