using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.DataObjects;
using MobileApp.Common.Specifications.Managers;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.BusinessLogic.Managers {

    internal class SslStreamTunnel : IEncryptedTunnel {

        private SslStream _sslStream;

        public SslStreamTunnel(SslStream sslStream) {
            _sslStream = sslStream;
        }

        public Task<byte[]> ReceiveData() {
            return Task.Run(() => {
                return DataAccess.Communication.SslTcpClient.ReadMessage(_sslStream);
            });
        }

        public Task SendData(byte[] msg) {
            return Task.Run(() => {
                DataAccess.Communication.SslTcpClient.SendMessage(_sslStream, msg);
            });
        }
    }

    public class RelayManager : IRelayManager {

        private SslStream _externalServerStream;

        private IPEndPoint _peerToPeerEndpoint;


        private ISslTcpClient SslTcpClient;

        private ISettingsManager SettingsManager;

        private IAesTcpClient AesTcpClient;

        private IApiRequestsRelayServer ApiRequestsRelayServer;

        private ICommandsRelayServer CommandsRelayServer;

        private ILogger Logger;

        public RelayManager(ILoggerService loggerService, ISslTcpClient sslTcpClient, ISettingsManager settingsManager, IAesTcpClient aesTcpClient, 
            IApiRequestsRelayServer apiRequestsRelayServer, ICommandsRelayServer commandsRelayServer) {
            Logger = loggerService.GetLogger<RelayManager>();
            SslTcpClient = sslTcpClient;
            SettingsManager = settingsManager;
            AesTcpClient = aesTcpClient;
            ApiRequestsRelayServer = apiRequestsRelayServer;
            CommandsRelayServer = commandsRelayServer;
        }

        public async Task<bool> ConnectToTheBasestation(CancellationToken cancellationToken) {
            bool success = false;
            var settings = await SettingsManager.GetApplicationSettings();

            // client must have been connected to the basestation at least one time before
            if (settings.BasestationId != Guid.Empty && settings.AesKey != null && settings.AesIV != null) {
                var config = ConfigurationStore.GetConfig();
                var ip = Dns.GetHostAddresses(config.ConnectionSettings.ExternalServer_Domain).FirstOrDefault();
                if (ip != null) {
                    success = await SslTcpClient.RunClient(new IPEndPoint(ip, Convert.ToInt32(config.ConnectionSettings.ExternalServer_RelayPort)), (sslStream) => {
                        // send basestation id
                        DataAccess.Communication.SslTcpClient.SendMessage(sslStream, settings.BasestationId.ToByteArray());

                        // receive relay request result
                        var rrrBytes = DataAccess.Communication.SslTcpClient.ReadMessage(sslStream);
                        var requestResult = JsonConvert.DeserializeObject<IRelayRequestResult>(Encoding.UTF8.GetString(rrrBytes));

                        _peerToPeerEndpoint = requestResult.BasestaionEndPoint;

                        // send back ack
                        DataAccess.Communication.SslTcpClient.SendMessage(sslStream, CommunicationCodes.ACK);

                        if (!requestResult.BasestationNotReachable && requestResult.BasestaionEndPoint == null) {
                            // packages will get relayed over the external server
                            _externalServerStream = sslStream;
                        }
                        else {
                            sslStream?.Close();
                        }

                    }, selfSignedCertificate: false, closeConnectionAfterCallback: false);
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
                    var sslStreamTunnel = new SslStreamTunnel(_externalServerStream);
                    success = await InitiateRelayingOutgoingPackages(sslStreamTunnel, cancellationToken);
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
