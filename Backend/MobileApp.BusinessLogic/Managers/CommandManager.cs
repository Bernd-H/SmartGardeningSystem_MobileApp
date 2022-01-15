using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Exceptions;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using Newtonsoft.Json;
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
                try {
                    success = false;

                    // send command
                    await AesTcpClient.SendData(CommunicationCodes.WlanCommand);

                    // receive ack
                    if ((await AesTcpClient.ReceiveData()).SequenceEqual(CommunicationCodes.ACK)) {
                        // send connect information
                        string connectInfo_json = JsonConvert.SerializeObject(wlanInfo);
                        await AesTcpClient.SendData(Encoding.UTF8.GetBytes(connectInfo_json));

                        // receive return code
                        try {
                            success = BitConverter.ToBoolean(await AesTcpClient.ReceiveData(), 0);
                        } catch (ConnectionClosedException) {
                            // return code won't get received if CommandServer connected to another wlan successfully
                            success = true;
                        }
                    }
                } catch (Exception ex) {
                    success = false;
                    Logger.Error(ex, $"[ConnectToWlan]An exception occoured while sending wlan-command-information.");
                }
            }

            AesTcpClient.Stop();
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
                var basestationIP = await GetBasestationIP();
                if (!string.IsNullOrEmpty(basestationIP)) {
                    int port = ConfigurationStore.GetConfig().ConnectionSettings.CommandsListener_Port;
                    return await AesTcpClient.Start(new IPEndPoint(IPAddress.Parse(basestationIP), port), receiveTimeout: 3*60*1000);
                }

                return false;
            }

            return true;
        }

        private async Task<string> GetBasestationIP() {
            if (SettingsManager.GetRuntimeVariables().RelayModeActive) {
                return "127.0.0.1";
            }
            else {
                return (await SettingsManager.GetApplicationSettings()).BaseStationIP;
            }
        }
    }
}
