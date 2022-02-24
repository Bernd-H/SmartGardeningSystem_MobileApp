using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
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

        private ISecureStorage SecureStorage;

        private IAesHandlerWithoutSettings AesEncrypterDecrypter;

        private string settingsFilePath;

        private byte[] cryptoKey;

        private byte[] cryptoIV;

        public bool accessedSecureStorage { get; set; } = false;

        private static SemaphoreSlim LOCKER = new SemaphoreSlim(1);

        private static GlobalRuntimeVariables globalRuntimeVariables;
        private static SemaphoreSlim globalRuntimeVarsLOCKER = new SemaphoreSlim(1);

        public SettingsManager(ILoggerService logger, IFileStorage fileStorage, ISecureStorage secureStorage, IAesHandlerWithoutSettings aesEncrypterDecrypter) { 
            Logger = logger.GetLogger<SettingsManager>();
            FileStorage = fileStorage;
            SecureStorage = secureStorage;
            AesEncrypterDecrypter = aesEncrypterDecrypter;

            setFilePathIfEmpty();
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
            if (!accessedSecureStorage) {
                // try loading/saving the aes key with which the settings file gets encrypted/decrypted
                var aesKeyLoaded = await getCryptoKey();
                if (!aesKeyLoaded) {
                    await trySaveCryptoKey();
                }

                accessedSecureStorage = true;
            }

            await createDefaultSettingsByMissingFile();

            //var settingsRaw = await FileStorage.ReadAsString(settingsFilePath);
            var settingsRaw = await getRawSettingsFile();
            var settings = JsonConvert.DeserializeObject<ApplicationSettings>(settingsRaw);

            return settings.ToDto();
        }

        private async Task updateSettings(ApplicationSettingsDto newSettings) {
            var jsonSettings = JsonConvert.SerializeObject(newSettings.FromDto());
            //await FileStorage.WriteAllText(settingsFilePath, jsonSettings);
            await writeRawSettingsFile(jsonSettings);
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

        private async Task<string> getRawSettingsFile() {
            var rawSettingsFile = await FileStorage.Read(settingsFilePath);
            if (cryptoKey != null) {
                try {
                    // decrypt file
                    return Encoding.UTF8.GetString(AesEncrypterDecrypter.Decrypt(rawSettingsFile, cryptoKey, cryptoIV));
                }
                catch (Exception) {
                    // wrong aes key
                    // override the current settings file with a new one
                    Logger.Error($"[getRawSettingsFile]Could not decrypt settings file. Deleting current settings.");
                    await updateSettings(ApplicationSettingsDto.GetStandardSettings());
                    return await getRawSettingsFile(); // warning: recurrent call... (should cause no problems though)
                }
            }

            return Encoding.UTF8.GetString(rawSettingsFile);
        }

        private async Task writeRawSettingsFile(string textToSave) {
            if (cryptoKey != null) {
                // encrypt file
                var encryptedFile = AesEncrypterDecrypter.Encrypt(Encoding.UTF8.GetBytes(textToSave), cryptoKey, cryptoIV);
                await FileStorage.Write(settingsFilePath, encryptedFile);
            }
            else {
                await FileStorage.WriteAllText(settingsFilePath, textToSave);
            }
        }

        private async Task<bool> getCryptoKey() {
            Logger.Trace($"[getCryptoKey]Requesting the aes key from the secure storage.");

            var configuration = ConfigurationStore.GetConfig();
            int keyLength = Convert.ToInt32(configuration.AesKeyLength_Bytes), ivLength = Convert.ToInt32(configuration.AesIvLength_Bytes);

            // load the cryptokey from secure storage
            var keyAndIV = await SecureStorage.Read(configuration.FileNames.SecureStorageKey_SettingsFileEncryptionKey);
            if (!string.IsNullOrEmpty(keyAndIV)) {
                var keyAndIVBytes = Convert.FromBase64String(keyAndIV);

                cryptoKey = new byte[keyLength];
                cryptoIV = new byte[ivLength];
                Array.Copy(keyAndIVBytes, 0, cryptoKey, 0, keyLength);
                Array.Copy(keyAndIVBytes, keyLength, cryptoIV, 0, ivLength);
                return true;
            }

            return false;
        }

        private async Task<bool> trySaveCryptoKey() {
            var configuration = ConfigurationStore.GetConfig();

            (var keyBytes, var ivBytes) = AesEncrypterDecrypter.CreateAesKeyIv();
            byte[] keyAndIvBytes = new byte[keyBytes.Length + ivBytes.Length];
            Array.Copy(keyBytes, 0, keyAndIvBytes, 0, keyBytes.Length);
            Array.Copy(ivBytes, 0, keyAndIvBytes, keyBytes.Length, ivBytes.Length);

            string keyAndIvString = Convert.ToBase64String(keyAndIvBytes);

            bool success = await SecureStorage.Write(configuration.FileNames.SecureStorageKey_SettingsFileEncryptionKey, keyAndIvString);
            if (success) {
                Logger.Info($"[trySaveCryptoKey]Saved the aes key in the secure storage successfully.");

                // set local key variables
                cryptoKey = keyBytes;
                cryptoIV = ivBytes;
            }
            else {
                Logger.Warn($"[trySaveCryptoKey]Could not save the aes key in the secure storage! (Storing settings unencrypted...)");
            }

            return success;
        }
    }
}
