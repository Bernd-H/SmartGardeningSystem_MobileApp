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

        public async Task<byte[]> ReceiveData() {
            Logger.Info($"[ReceiveData]Waiting to receive data from {tcpClient.Client.RemoteEndPoint.ToString()}.");

            byte[] packet = await CommunicationUtils.ReceiveAsync(Logger, networkStream);

            // decrypt message
            byte[] decryptedPacket = AesEncrypterDecrypter.Decrypt(packet, AesKey, AesIV);

            return decryptedPacket;
        }

        public async Task<byte[]> SendAndReceiveData(byte[] msg) {
            await LOCKER.WaitAsync();

            await SendData(msg);
            var received = await ReceiveData();

            LOCKER.Release();
            return received;
        }

        public async Task SendData(byte[] msg) {
            Logger.Info($"[SendData] Sending data with length {msg.Length}.");

            // encrypt message
            byte[] encryptedPacket = AesEncrypterDecrypter.Encrypt(msg, AesKey, AesIV);

            await CommunicationUtils.SendAsync(Logger, encryptedPacket, networkStream);
        }

        public async Task<bool> Start(IPEndPoint remoteEndPoint) {
            try {
                // get aes key + iv
                var settings = await SettingsManager.GetApplicationSettings();
                AesIV = settings.AesIV;
                AesKey = settings.AesKey;

                tcpClient = new TcpClient();
                tcpClient.SendTimeout = 1000; // 1s
                tcpClient.Connect(remoteEndPoint);
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
