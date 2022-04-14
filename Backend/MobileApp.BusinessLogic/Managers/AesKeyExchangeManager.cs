using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using NLog;

namespace MobileApp.BusinessLogic.Managers {

    /// <inheritdoc/>
    public class AesKeyExchangeManager : IAesKeyExchangeManager {

        private ILogger Logger;

        private ISettingsManager SettingsManager;

        private ISslTcpClient SslTcpClient;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        private IAPIManager APIManager;

        public AesKeyExchangeManager(ILoggerService loggerService, ISettingsManager settingsManager, ISslTcpClient sslTcpClient, 
            IAPIManager apiManager) {
            Logger = loggerService.GetLogger<AesKeyExchangeManager>();
            SettingsManager = settingsManager;
            SslTcpClient = sslTcpClient;
            APIManager = apiManager;
        }

        /// <inheritdoc/>
        public Task<bool> Start(CancellationToken token) {
            return Task<bool>.Run(async () => {
                bool success = false;
                try {
                    // try to connect to server and exchange aes keys
                    //string serverIP = ConfigurationStore.GetConfig().ConnectionSettings.ConfigurationWiFi_ServerIP;
                    string serverIP = (await SettingsManager.GetApplicationSettings()).BaseStationIP;
                    int port = Convert.ToInt32(ConfigurationStore.GetConfig().ConnectionSettings.ConfigurationWiFi_KeyExchangeListenPort);
                    var endPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);

                    Logger.Info($"[Start]No aes key stored. Trying to connect to server {endPoint.ToString()}.");

                    success = await SslTcpClient.Start(endPoint);
                    if (success) {
                        // how do we know if we are connected to the accesspoint of the basestation?
                        //if (APIManager.IsBasestationConnectedToWlan().Result) {
                        //    Logger.Warn($"[sslStreamOpenCallback]Key exchange aborted. This key exchanges is only allowed when connected to the access point of the basestation directly.");
                        //    return;
                        //}

                        Logger.Info($"[Start]Waiting to receive aes key, iv and the certificate of the basestation.");

                        // receive key
                        var key = await SslTcpClient.ReceiveData();

                        // send ack
                        await SslTcpClient.SendData(CommunicationCodes.ACK);

                        // receive iv
                        var iv = await SslTcpClient.ReceiveData();

                        // send ack
                        await SslTcpClient.SendData(CommunicationCodes.ACK);

                        // save the received key information
                        Logger.Info($"[Start]Saving the received aes key.");
                        await SettingsManager.UpdateCurrentSettings(currentSettings => {
                            currentSettings.AesKey = key;
                            currentSettings.AesIV = iv;
                            currentSettings.BasestationCert = SslTcpClient.GetServerCert();
                            return currentSettings;
                        });
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, "[Start]An error occured while trying to get the aes key from the basestation.");
                    success = false;
                }

                return success;
            }, token);
        }
    }
}
