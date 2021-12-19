using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.DataObjects;
using MobileApp.Common.Specifications.Managers;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.BusinessLogic.Managers {
    public class RelayManager : IRelayManager {

        private SslStream _externalServerStream;

        private IPEndPoint _peerToPeerEndpoint;


        private ISslTcpClient SslTcpClient;

        private ISettingsManager SettingsManager;

        private IAesTcpClient AesTcpClient;

        private IAesTunnelInSslStream AesTunnelInSslStream;

        private IApiRequestsRelayServer ApiRequestsRelayServer;

        private ICommandsRelayServer CommandsRelayServer;

        private ILogger Logger;

        public RelayManager(ILoggerService loggerService, ISslTcpClient sslTcpClient, ISettingsManager settingsManager, IAesTcpClient aesTcpClient, 
            IApiRequestsRelayServer apiRequestsRelayServer, ICommandsRelayServer commandsRelayServer, IAesTunnelInSslStream aesTunnelInSslStream) {
            Logger = loggerService.GetLogger<RelayManager>();
            SslTcpClient = sslTcpClient;
            SettingsManager = settingsManager;
            AesTcpClient = aesTcpClient;
            ApiRequestsRelayServer = apiRequestsRelayServer;
            CommandsRelayServer = commandsRelayServer;
            AesTunnelInSslStream = aesTunnelInSslStream;
        }

        public async Task<bool> ConnectToTheBasestation(CancellationToken cancellationToken) {
            bool success = false;
            var settings = await SettingsManager.GetApplicationSettings();

            string targetHost = ConfigurationStore.GetConfig().ConnectionSettings.ExternalServer_Domain;

            // client must have been connected to the basestation at least one time before
            if (settings.BasestationId != Guid.Empty && settings.AesKey != null && settings.AesIV != null) {
                var config = ConfigurationStore.GetConfig();
                var ip = Dns.GetHostAddresses(config.ConnectionSettings.ExternalServer_Domain).FirstOrDefault();
                if (ip != null) {
                    success = await SslTcpClient.RunClient(new IPEndPoint(ip, Convert.ToInt32(config.ConnectionSettings.ExternalServer_RelayPort)), (sslStream) => {                        
                        // send basestation id
                        SslTcpClient.SendMessage(sslStream, settings.BasestationId.ToByteArray());

                        // receive relay request result
                        var rrrBytes = SslTcpClient.ReadMessage(sslStream);
                        var requestResult = JsonConvert.DeserializeObject<RelayRequestResult>(Encoding.UTF8.GetString(rrrBytes));

                        _peerToPeerEndpoint = requestResult.BasestaionEndPoint;

                        // send back ack
                        SslTcpClient.SendMessage(sslStream, CommunicationCodes.ACK);

                        if (!requestResult.BasestationNotReachable && requestResult.BasestaionEndPoint == null) {
                            // packages will get relayed over the external server
                            _externalServerStream = sslStream;
                        }
                        else {
                            sslStream?.Close();
                        }

                    }, selfSignedCertificate: false, closeConnectionAfterCallback: false, targetHost);
                }
                else {
                    Logger.Error($"[ConnectToTheBasestation]Could not resolve domain {config.ConnectionSettings.ExternalServer_Domain}.");
                }
            }

            if (success) {
                if (_externalServerStream == null) {
                    // connect to the basestation with the given endpoint
                    success = await AesTcpClient.Start(_peerToPeerEndpoint);
                    if (success) {
                        success = await InitiateRelayingOutgoingPackages(AesTcpClient, cancellationToken);
                    }
                } else {
                    // use existing connection to relay messages to the basestation
                    AesTunnelInSslStream.Init(_externalServerStream);
                    success = await InitiateRelayingOutgoingPackages(AesTunnelInSslStream, cancellationToken);
                }
            }

            return success;
        }

        private async Task<bool> InitiateRelayingOutgoingPackages(IEncryptedTunnel encryptedTunnel, CancellationToken cancellationToken) {
            bool apiServerStarted = false, commandServerStarted = false;

            // start local relay servers
            apiServerStarted = await ApiRequestsRelayServer.Start(encryptedTunnel, cancellationToken);
            commandServerStarted = await CommandsRelayServer.Start(encryptedTunnel, cancellationToken);

            // set RelayMode so that API- and CommandManager will send their packages to the local relay server
            SettingsManager.UpdateCurrentRuntimeVariables((runtimeSettings) => {
                runtimeSettings.RelayModeActive = true;
                return runtimeSettings;
            });

            return apiServerStarted && commandServerStarted;
        }
    }
}
