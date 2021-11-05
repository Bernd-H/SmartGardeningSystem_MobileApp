using System;
using System.Net;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using NLog;

namespace MobileApp.BusinessLogic.Managers {
    public class CommandManager : ICommandManager, IDisposable {

        private ILogger Logger;

        private IAesTcpClient AesTcpClient;

        private ISettingsManager SettingsManager;

        public CommandManager(ILoggerService loggerService, IAesTcpClient aesTcpClient, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<CommandManager>();
            AesTcpClient = aesTcpClient;
            SettingsManager = settingsManager;
        }
        public async Task<bool> ConnectToWlan(WlanInfoDto wlanInfo) {
            var success = await StartConnection();
            if (success) {
                throw new NotImplementedException();
            }

            return success;
        }

        public void Dispose() {
            AesTcpClient.Dispose();
        }

        /// <summary>
        /// initiates the connection with the server.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> StartConnection() {
            if (!AesTcpClient.IsConnected()) {
                // get ipendpoint to connect to
                var settings = await SettingsManager.GetApplicationSettings();
                if (!string.IsNullOrEmpty(settings.BaseStationIP)) {
                    int port = ConfigurationStore.GetConfig().ConnectionSettings.CommandsListener_Port;
                    return await AesTcpClient.Start(new IPEndPoint(IPAddress.Parse(settings.BaseStationIP), port));
                }

                return false;
            }

            return true;
        }
    }
}
