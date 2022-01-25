using System;
using System.IO;
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
using MobileApp.Common.Utilities;
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
            Logger.Info($"[ConnectToWlan]Transmitting wlan ssid & secret to basestation.");
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

        public async Task<bool> DisconnectFromWlan() {
            Logger.Info($"[DisconnectFromWlan]Sending disconnect command to the basestation.");
            var success = await StartConnection();
            if (success) {
                try {
                    success = false;

                    // send command
                    await AesTcpClient.SendData(CommunicationCodes.DisconnectFromWlanCommand);

                    // receive ack
                    if ((await AesTcpClient.ReceiveData()).SequenceEqual(CommunicationCodes.ACK)) {
                        // receive return code
                        try {
                            success = BitConverter.ToBoolean(await AesTcpClient.ReceiveData(), 0);
                        }
                        catch (ConnectionClosedException) {
                            // the return code won't get received if the basestation disconnected form the wlan successfully.
                            success = true;
                        }
                    }
                }
                catch (Exception ex) {
                    success = false;
                    Logger.Error(ex, $"[ConnectToWlan]An exception occoured while sending the disconnect command.");
                }
            }

            AesTcpClient.Stop();
            return success;
        }

        public Task<bool> StartAutomaticIrrigation() {
            Logger.Info($"[StartAutomaticIrrigation]Sending start automatic irrigation command.");
            return sendCommand(CommunicationCodes.StartAutomaticIrrigationCommand);
        }

        public Task<bool> StopAutomaticIrrigation() {
            Logger.Info($"[StopAutomaticIrrigation]Sending stop automatic irrigation command.");
            return sendCommand(CommunicationCodes.StopAutomaticIrrigationCommand);
        }

        public Task<bool> StartManualIrrigation(TimeSpan timeSpan) {
            Logger.Info($"[StartManualIrrigation]Sending start manual irrigation command.");
            return sendCommand(CommunicationCodes.StartManualIrrigationCommand, openConnectionAction: () => {
                // send TimeSpan
                var timeSpanMinutes = BitConverter.GetBytes(timeSpan.TotalMinutes);
                AesTcpClient.SendData(timeSpanMinutes);
            });
        }

        public Task<bool> StopManualIrrigation() {
            Logger.Info($"[StopManualIrrigation]Sending stop manual irrigation command.");
            return sendCommand(CommunicationCodes.StopManualIrrigationCommand);
        }

        public void Dispose() {
            AesTcpClient.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">Command from CommunicationsCodes</param>
        /// <param name="openConnectionAction">To send additional information if neccessary</param>
        /// <returns>True, when success</returns>
        private async Task<bool> sendCommand(byte[] command, Action openConnectionAction = null) {
            var success = await StartConnection();
            if (success) {
                try {
                    success = false;

                    // send command
                    await AesTcpClient.SendData(command);

                    // receive ack
                    if ((await AesTcpClient.ReceiveData()).SequenceEqual(CommunicationCodes.ACK)) {
                        openConnectionAction?.Invoke();

                        // receive return code
                        success = BitConverter.ToBoolean(await AesTcpClient.ReceiveData(), 0);
                    }
                }
                catch (Exception ex) {
                    success = false;
                    Logger.Error(ex, $"[sendCommand]An exception occoured while sending command {Utils.ConvertByteArrayToHex(command)}.");
                }
            }

            AesTcpClient.Stop();
            return success;
        }

        /// <summary>
        /// initiates the connection with the server.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> StartConnection() {
            if (!AesTcpClient.IsConnected()) {
                Logger.Trace($"[StartConnection]Connecting to the basestation."); 

                // get ipendpoint to connect to
                var basestationIP = await GetBasestationIP();
                if (!string.IsNullOrEmpty(basestationIP)) {
                    int port = ConfigurationStore.GetConfig().ConnectionSettings.CommandsListener_Port;
                    return await AesTcpClient.Start(new IPEndPoint(IPAddress.Parse(basestationIP), port), receiveTimeout: 3*60*1000);
                }

                return false;
            }
            else {
                Logger.Trace($"[StartConnection]Already connectd to the basestation.");
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
