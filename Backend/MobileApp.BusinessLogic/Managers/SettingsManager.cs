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

            var settings = await getApplicationSettings();

            LOCKER.Release();
            return settings;
        }

        public async Task UpdateCurrentSettings(Func<ApplicationSettingsDto, ApplicationSettingsDto> updateFunc) {
            await LOCKER.WaitAsync();

            await updateSettings(updateFunc(await getApplicationSettings()));

            LOCKER.Release();
        }

        private async Task<ApplicationSettingsDto> getApplicationSettings() {
            Logger.Trace("[GetApplicationSettings]Loading application settings.");
            setFilePathIfEmpty();
            await createDefaultSettingsByMissingFile();

            var settingsRaw = FileStorage.ReadAsString(settingsFilePath).Result;
            var settings = JsonConvert.DeserializeObject<ApplicationSettingsDto>(settingsRaw);

            return settings;
        }

        private async Task updateSettings(ApplicationSettingsDto newSettings) {
            Logger.Trace("[UpdateSettings]Writing to application settings.");

            var jsonSettings = JsonConvert.SerializeObject(newSettings);
            await FileStorage.WriteAllText(settingsFilePath, jsonSettings);
        }

        private async Task createDefaultSettingsByMissingFile() {
            if (!File.Exists(settingsFilePath)) {
                Logger.Info("[SettingsManager]Creating default settings file.");

                // create default settings file
                await updateSettings(ApplicationSettingsDto.GetStandardSettings());
            }
        }

        private void setFilePathIfEmpty() {
            if (string.IsNullOrEmpty(settingsFilePath)) {
                var configuration = ConfigurationStore.GetConfig();
                settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), configuration.FileNames.SettingsFileName);
                //settingsFilePath = configuration.FileNames.SettingsFileName;
            }
        }
    }
}
