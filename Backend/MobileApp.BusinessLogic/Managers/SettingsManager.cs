using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using Newtonsoft.Json;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.Managers {

    /// <inheritdoc/>
    public class SettingsManager : ISettingsManager {

        public static bool PLATFORM_IS_WINDOWS = false;


        private ILogger Logger;

        private IFileStorage FileStorage;

        private ISecureStorage SecureStorage; // only store in secureStorage when not on windows....

        private string settingsFilePath;

        private static SemaphoreSlim LOCKER = new SemaphoreSlim(1);

        private static GlobalRuntimeVariables globalRuntimeVariables;
        private static SemaphoreSlim globalRuntimeVarsLOCKER = new SemaphoreSlim(1);

        public SettingsManager(ILoggerService logger, IFileStorage fileStorage, ISecureStorage secureStorage) { 
            Logger = logger.GetLogger<SettingsManager>();
            FileStorage = fileStorage;
            SecureStorage = secureStorage;
        }

        #region runtime variables

        /// <inheritdoc/>
        public GlobalRuntimeVariables GetRuntimeVariables() {
            Logger.Trace($"[GetRuntimeVariables]Requested GlobalRuntimeVariables.");
            if (globalRuntimeVariables == null) {
                globalRuntimeVariables = new GlobalRuntimeVariables();
            }

            return globalRuntimeVariables;
        }

        /// <inheritdoc/>
        public void UpdateCurrentRuntimeVariables(Func<GlobalRuntimeVariables, GlobalRuntimeVariables> updateFunc) {
            globalRuntimeVarsLOCKER.Wait();
            Logger.Trace($"[UpdateCurrentRuntimeVariables]Updating GlobalRuntimeVariables.");
            globalRuntimeVariables = updateFunc(GetRuntimeVariables());
            globalRuntimeVarsLOCKER.Release();
        }

        #endregion

        /// <inheritdoc/>
        public async Task<ApplicationSettingsDto> GetApplicationSettings() {
            await LOCKER.WaitAsync();

            Logger.Trace($"[GetApplicationSettings]Requested application settings.");
            var settings = await getApplicationSettings();

            LOCKER.Release();
            return settings;
        }

        /// <inheritdoc/>
        public async Task UpdateCurrentSettings(Func<ApplicationSettingsDto, ApplicationSettingsDto> updateFunc) {
            await LOCKER.WaitAsync();

            Logger.Trace($"[UpdateCurrentSettings]Updateing current application settings.");
            await updateSettings(updateFunc(await getApplicationSettings()));

            LOCKER.Release();
        }

        private async Task<ApplicationSettingsDto> getApplicationSettings() {
            Logger.Trace("[getApplicationSettings]Loading application settings.");
            setFilePathIfEmpty();
            await createDefaultSettingsByMissingFile();

            var settingsRaw = FileStorage.ReadAsString(settingsFilePath).Result;
            var settings = JsonConvert.DeserializeObject<ApplicationSettings>(settingsRaw);

            return settings.ToDto();
        }

        private async Task updateSettings(ApplicationSettingsDto newSettings) {
            var jsonSettings = JsonConvert.SerializeObject(newSettings.FromDto());
            await FileStorage.WriteAllText(settingsFilePath, jsonSettings);
        }

        private async Task createDefaultSettingsByMissingFile() {
            if (!File.Exists(settingsFilePath)) {
                Logger.Info("[createDefaultSettingsByMissingFile]Creating default settings file.");

                // create default settings file
                await updateSettings(ApplicationSettingsDto.GetStandardSettings());
            }
        }

        private void setFilePathIfEmpty() {
            if (string.IsNullOrEmpty(settingsFilePath)) {
                var configuration = ConfigurationStore.GetConfig();
                if (PLATFORM_IS_WINDOWS) {
                    // -> Tests folder
                    settingsFilePath = configuration.FileNames.SettingsFileName;
                }
                else {
                    settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), configuration.FileNames.SettingsFileName);
                }
            }
        }
    }
}
