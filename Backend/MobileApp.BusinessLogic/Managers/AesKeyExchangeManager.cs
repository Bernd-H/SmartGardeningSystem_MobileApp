using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using NLog;

namespace MobileApp.BusinessLogic.Managers {
    public class AesKeyExchangeManager : IAesKeyExchangeManager {

        private ILogger Logger;

        private ISettingsManager SettingsManager;

        private ISslTcpClient SslTcpClient;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;


        public AesKeyExchangeManager(ILoggerService loggerService, ISettingsManager settingsManager, ISslTcpClient sslTcpClient) {
            Logger = loggerService.GetLogger<AesKeyExchangeManager>();
            SettingsManager = settingsManager;
            SslTcpClient = sslTcpClient;
        }

        public Task<bool> Start(CancellationToken token) {
            return Task.Run(async () => {
                // try to connect to server and exchange aes keys
                //string serverIP = ConfigurationStore.GetConfig().ConnectionSettings.ConfigurationWiFi_ServerIP;
                string serverIP = (await SettingsManager.GetApplicationSettings()).BaseStationIP;
                int port = Convert.ToInt32(ConfigurationStore.GetConfig().ConnectionSettings.ConfigurationWiFi_KeyExchangeListenPort);
                var endPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);

                Logger.Info($"[Start]No aes key stored. Trying to connect to server {endPoint.ToString()}.");

                return await SslTcpClient.RunClient(endPoint, sslStreamOpenCallback);
            }, token);
        }

        private void sslStreamOpenCallback(SslStream openStream) {
            Logger.Info($"[sslStreamOpenCallback]Waiting to receive aes key and iv.");

            // receive key
            var key = DataAccess.Communication.SslTcpClient.ReadMessage(openStream);

            // send ack
            DataAccess.Communication.SslTcpClient.SendMessage(openStream, CommunicationCodes.ACK);

            // receive iv
            var iv = DataAccess.Communication.SslTcpClient.ReadMessage(openStream);

            // send ack
            DataAccess.Communication.SslTcpClient.SendMessage(openStream, CommunicationCodes.ACK);

            // save the received key information
            Logger.Info($"[sslStreamOpenCallback]Saving the received aes key information.");
            SettingsManager.UpdateCurrentSettings(currentSettings => {
                currentSettings.AesKey = key;
                currentSettings.AesIV = iv;
                return currentSettings;
            });
        }
    }
}
