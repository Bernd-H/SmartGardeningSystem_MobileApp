using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using NLog;
using Xamarin.Essentials;

namespace MobileApp.DataAccess {

    /// <inheritdoc/>
    public class SecureDataStorage : ISecureStorage { // is locking needed?

        private ILogger Logger;

        public SecureDataStorage(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<SecureDataStorage>();
        }

        /// <inheritdoc/>
        public async Task<string> Read(string key) {
            string result = string.Empty;
            try {
                result = await SecureStorage.GetAsync(key);
            }
            catch (Exception ex) {
                // Possible that device doesn't support secure storage on device.
                Logger.Error(ex, "[Read]An error occured.");
            }

            return result;
        }

        /// <inheritdoc/>
        public bool Remove(string key) {
            try {
                return SecureStorage.Remove(key);
            }
            catch (Exception ex) {
                // Possible that device doesn't support secure storage on device.
                Logger.Error(ex, "[Remove]An error occured.");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> Write(string key, string value) {
            try {
                await SecureStorage.SetAsync(key, value);
            }
            catch (Exception ex) {
                // Possible that device doesn't support secure storage on device.
                Logger.Error(ex, "[Write]An error occured.");
                return false;
            }

            return true;
        }
    }
}
