using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.DataObjects;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Utilities;
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

        public async Task<bool> ConnectToTheBasestation(CancellationToken cancellationToken, bool forceRelay = false) {
            bool success = false;
            var settings = await SettingsManager.GetApplicationSettings();
            var config = ConfigurationStore.GetConfig();
            string targetHost = config.ConnectionSettings.ExternalServer_Domain;

            // client must have been connected to the basestation at least one time before
            if (settings.BasestationId != Guid.Empty && settings.AesKey != null && settings.AesIV != null) {
                var ip = IpUtils.GetHostAddress(config.ConnectionSettings.ExternalServer_Domain, 5000);
                if (ip != null) {
                    success = await SslTcpClient.RunClient(new IPEndPoint(ip, Convert.ToInt32(config.ConnectionSettings.ExternalServer_RelayPort)), (sslStream, remoteCert) => {
                        connectedCallback(sslStream, settings.BasestationId, forceRelay);
                    }, selfSignedCertificate: false, closeConnectionAfterCallback: false, targetHost);
                }
                else {
                    Logger.Error($"[ConnectToTheBasestation]Could not resolve domain {config.ConnectionSettings.ExternalServer_Domain}.");
                }
            }

            if (success) {
                if (_externalServerStream == null) {
                    // connect to the basestation with the given endpoint
                    success = await AesTcpClient.Start(_peerToPeerEndpoint, 5000);
                    if (success) {
                        success = await initiateRelayingOutgoingPackages(AesTcpClient, cancellationToken);
                    }
                    else if (forceRelay == false) {
                        // peer to peer connection didn't work.
                        // tell the external server to relay all traffic
                        return await ConnectToTheBasestation(cancellationToken, forceRelay: true);
                    }
                } else {
                    // use existing connection to relay messages to the basestation
                    AesTunnelInSslStream.Init(_externalServerStream);
                    success = await initiateRelayingOutgoingPackages(AesTunnelInSslStream, cancellationToken);
                }
            }

            return success;
        }

        private void connectedCallback(SslStream sslStream, Guid basestationId, bool forceRelay) {
            // send basestation id
            SslTcpClient.SendMessage(sslStream, CommunicationUtils.SerializeObject<ConnectRequestDto>(new ConnectRequest {
                BasestationId = basestationId,
                ForceRelay = forceRelay
            }.ToDto()));

            // receive relay request result
            var rrrBytes = SslTcpClient.ReadMessage(sslStream);
            var requestResult = CommunicationUtils.DeserializeObject<ConnectRequestResultDto>(rrrBytes).FromDto();

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
        }

        private async Task<bool> initiateRelayingOutgoingPackages(IEncryptedTunnel encryptedTunnel, CancellationToken cancellationToken) {
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
