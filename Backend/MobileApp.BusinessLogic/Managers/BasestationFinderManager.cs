using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using NLog;

namespace MobileApp.BusinessLogic.Managers {

    /// <inheritdoc/>
    public class BasestationFinderManager : IBasestationFinderManager, IDisposable {

        private ILocalBasestationDiscovery LocalBasestationDiscovery;

        private ISettingsManager SettingsManager;

        private ILogger Logger;

        public BasestationFinderManager(ILoggerService loggerService, ILocalBasestationDiscovery localBasestationDiscovery, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<BasestationFinderManager>();
            LocalBasestationDiscovery = localBasestationDiscovery;
            SettingsManager = settingsManager;
        }

        ~BasestationFinderManager() {
            Dispose();
        }

        /// <inheritdoc/>
        public void Dispose() {
            LocalBasestationDiscovery.Dispose();
        }

        /// <inheritdoc/>
        public async Task<bool> FindLocalBaseStation() {
            BasestationFoundDto baseStationInfo = null;
            int attempts = 1;

            do {
                Logger.Info($"[FindLocalBaseStation]Trying to find basestation attempt {1 - attempts}.");
                baseStationInfo = await LocalBasestationDiscovery.TryFindBasestation();

                attempts--;
            } while (attempts > 0 && baseStationInfo == null);

            if (baseStationInfo != null) {
                // store basestation ip address and id
                await SettingsManager.UpdateCurrentSettings(currentSettings => {
                    currentSettings.BaseStationIP = baseStationInfo.RemoteEndPoint.Address.ToString();
                    currentSettings.BasestationId = baseStationInfo.Id;
                    return currentSettings;
                });
            }

            return baseStationInfo != null;
        }
    }
}
