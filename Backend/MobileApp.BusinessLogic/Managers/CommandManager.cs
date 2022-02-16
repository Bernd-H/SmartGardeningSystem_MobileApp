using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Exceptions;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Utilities;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.BusinessLogic.Managers {

    /// <inheritdoc/>
    public class CommandManager : ICommandManager, IDisposable {

        private ILogger Logger;

        private IAesTcpClient AesTcpClient;

        private ISettingsManager SettingsManager;

        public CommandManager(ILoggerService loggerService, IAesTcpClient aesTcpClient, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<CommandManager>();
            AesTcpClient = aesTcpClient;
            SettingsManager = settingsManager;
        }

        /// <inheritdoc/>
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
                        await AesTcpClient.SendData(CommunicationUtils.SerializeObject(wlanInfo));

                        // receive return code
                        try {
                            success = BitConverter.ToBoolean(await AesTcpClient.ReceiveData(), 0);
                        } catch (Exception rrc_ex) {
                            if (rrc_ex.GetType() == typeof(ConnectionClosedException) || rrc_ex.GetType() == typeof(SocketException)) {
                                // return code won't get received if CommandServer connected to another wlan successfully
                                success = true;
                            }
                            else {
                                throw;
                            }
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Task<bool> StartAutomaticIrrigation() {
            Logger.Info($"[StartAutomaticIrrigation]Sending start automatic irrigation command.");
            return sendCommand(CommunicationCodes.StartAutomaticIrrigationCommand);
        }

        /// <inheritdoc/>
        public Task<bool> StopAutomaticIrrigation() {
            Logger.Info($"[StopAutomaticIrrigation]Sending stop automatic irrigation command.");
            return sendCommand(CommunicationCodes.StopAutomaticIrrigationCommand);
        }

        /// <inheritdoc/>
        public Task<bool> StartManualIrrigation(TimeSpan timeSpan) {
            Logger.Info($"[StartManualIrrigation]Sending start manual irrigation command.");
            return sendCommand(CommunicationCodes.StartManualIrrigationCommand, openConnectionAction: new Func<Task>(async () => {
                // send TimeSpan
                var timeSpanMinutes = BitConverter.GetBytes(timeSpan.TotalMinutes);
                await AesTcpClient.SendData(timeSpanMinutes);
            }));
        }

        /// <inheritdoc/>
        public Task<bool> StopManualIrrigation() {
            Logger.Info($"[StopManualIrrigation]Sending stop manual irrigation command.");
            return sendCommand(CommunicationCodes.StopManualIrrigationCommand);
        }

        /// <inheritdoc/>
        public async Task<byte?> DiscoverNewModule(CancellationToken cancellationToken) {
            Logger.Info($"[DiscoverNewModule]Sending discover module command.");

            byte[] moduleInfoBytes = null;

            var success = await sendCommand(CommunicationCodes.DiscoverNewModuleCommand, openConnectionAction: new Func<Task>(async () => {
                // receive Module info
                moduleInfoBytes = await AesTcpClient.ReceiveData(cancellationToken);
            }));

            if (success && !cancellationToken.IsCancellationRequested) {
                try {
                    return CommunicationUtils.DeserializeObject<ModuleInfo>(moduleInfoBytes).ModuleId;
                }
                catch (Exception ex) {
                    Logger.Error(ex, $"[DiscoverNewModule]An error occured while deserializing the ModuleInfo object of the newly discovered module.");
                    return null;
                }
            }
            else {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<bool> Test() {
            Logger.Info($"[Test]Sending the test command.");
            return sendCommand(CommunicationCodes.Test);
        }

        /// <inheritdoc/>
        public void Dispose() {
            AesTcpClient.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">Command from CommunicationsCodes</param>
        /// <param name="openConnectionAction">To send additional information if neccessary</param>
        /// <returns>True, when success</returns>
        private async Task<bool> sendCommand(byte[] command, Func<Task> openConnectionAction = null, CancellationToken token = default) {
            var success = await StartConnection();
            if (success) {
                try {
                    success = false;

                    // send command
                    await AesTcpClient.SendData(command, token);

                    // receive ack
                    if ((await AesTcpClient.ReceiveData(token)).SequenceEqual(CommunicationCodes.ACK)) {
                        await (openConnectionAction?.Invoke() ?? Task.CompletedTask);

                        // receive return code
                        success = BitConverter.ToBoolean(await AesTcpClient.ReceiveData(token), 0);
                    }
                }
                catch (Exception ex) {
                    success = false;
                    if (token.IsCancellationRequested) {
                        Logger.Info($"[sendCommand]Cancellation got requested.");
                    }
                    else {
                        Logger.Error(ex, $"[sendCommand]An exception occoured while sending command {Utils.ConvertByteArrayToHex(command)}.");
                    }
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
