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

        public void Dispose() {
            LocalBasestationDiscovery.Dispose();
        }

        public async Task<bool> FindLocalBaseStation() {
            BasestationFoundDto baseStationInfo = null;
            int attempts = 3;

            do {
                Logger.Info($"[FindLocalBaseStation]Trying to find basestation attempt {3 - attempts}.");
                baseStationInfo = await LocalBasestationDiscovery.TryFindBasestation();

                attempts--;
            } while (attempts >= 0 && baseStationInfo == null);

            if (baseStationInfo != null) {
                // store basestation ip address
                await SettingsManager.UpdateCurrentSettings(currentSettings => {
                    currentSettings.BaseStationIP = baseStationInfo.RemoteEndPoint.Address.ToString();
                    return currentSettings;
                });
            }

            return baseStationInfo != null;
        }

        public static bool IsHostAvailable(string ipAddress) {
            bool pingable = false;
            Ping pinger = null;

            try {
                pinger = new Ping();
                PingReply reply = pinger.Send(ipAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException) {
                // Discard PingExceptions and return false;
            }
            finally {
                if (pinger != null) {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
    }
}
