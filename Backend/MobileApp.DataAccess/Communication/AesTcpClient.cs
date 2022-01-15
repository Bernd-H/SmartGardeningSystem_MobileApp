using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Utilities;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class AesTcpClient : IAesTcpClient, IDisposable, IEncryptedTunnel {

        private static SemaphoreSlim LOCKER = new SemaphoreSlim(1);


        private TcpClient tcpClient;

        private NetworkStream networkStream;

        private byte[] AesIV;

        private byte[] AesKey;


        private ILogger Logger;

        private ISettingsManager SettingsManager;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        public AesTcpClient(ILoggerService loggerService, ISettingsManager settingsManager, IAesEncrypterDecrypter aesEncrypterDecrypter) {
            Logger = loggerService.GetLogger<AesTcpClient>();
            SettingsManager = settingsManager;
            AesEncrypterDecrypter = aesEncrypterDecrypter;
        }

        public void Dispose() {
            networkStream?.Close();
            tcpClient?.Close();
        }

        public bool IsConnected() {
            return tcpClient?.Connected ?? false;
        }

        public async Task<byte[]> ReceiveData(CancellationToken cancellationToken = default) {
            Logger.Info($"[ReceiveData]Waiting to receive data from {tcpClient.Client.RemoteEndPoint.ToString()}.");

            byte[] packet = await CommunicationUtils.ReceiveAsync(Logger, networkStream, cancellationToken);
            if (cancellationToken.IsCancellationRequested) {
                return null;
            }

            // decrypt message
            byte[] decryptedPacket = AesEncrypterDecrypter.Decrypt(packet, AesKey, AesIV);

            return decryptedPacket;
        }

        public async Task<byte[]> SendAndReceiveData(byte[] msg, CancellationToken cancellationToken = default) {
            await LOCKER.WaitAsync();

            await SendData(msg, cancellationToken);
            var received = await ReceiveData(cancellationToken);

            LOCKER.Release();
            return received;
        }

        public async Task SendData(byte[] msg, CancellationToken cancellationToken = default) {
            Logger.Info($"[SendData] Sending data with length {msg.Length}.");

            // encrypt message
            byte[] encryptedPacket = AesEncrypterDecrypter.Encrypt(msg, AesKey, AesIV);

            await CommunicationUtils.SendAsync(Logger, encryptedPacket, networkStream, cancellationToken);
        }

        public async Task<bool> Start(IPEndPoint remoteEndPoint, int receiveTimeout) {
            try {
                // get aes key + iv
                var settings = await SettingsManager.GetApplicationSettings();
                AesIV = settings.AesIV;
                AesKey = settings.AesKey;

                tcpClient = new TcpClient();
                tcpClient.SendTimeout = 1000; // 1s
                tcpClient.ReceiveTimeout = receiveTimeout;
                tcpClient.Client.Blocking = true;
                //await tcpClient.ConnectAsync(remoteEndPoint.Address, remoteEndPoint.Port);
                CommunicationUtils.ConnectWithTimout(tcpClient.Client, remoteEndPoint, millisecondsTimeout: 2000);
                networkStream = tcpClient.GetStream();

                return true;
            }
            catch (Exception ex) {
                Logger.Error(ex, "[Start]Error while connecting to remote end point.");
            }

            return false;
        }

        public void Stop() {
            if (IsConnected()) {
                networkStream?.Close();
                tcpClient?.Close();
            }
        }
    }
}
