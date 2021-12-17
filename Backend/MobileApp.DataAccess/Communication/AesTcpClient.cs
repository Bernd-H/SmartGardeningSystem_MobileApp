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
            //try {
                Logger.Info($"[ReceiveData]Waiting to receive data from {tcpClient.Client.RemoteEndPoint.ToString()}.");
                int bytes = -1;
                int packetLength = -1;
                int readBytes = 0;
                List<byte> packet = new List<byte>();

                do {
                    byte[] buffer = new byte[2048];
                    bytes = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                    // get length
                    if (packetLength == -1) {
                        byte[] length = new byte[4];
                        Array.Copy(buffer, 0, length, 0, 4);
                        packetLength = BitConverter.ToInt32(length, 0);
                    }

                    readBytes += bytes;
                    packet.AddRange(buffer);

                } while (bytes != 0 && packetLength - readBytes > 0);

                // remove length information and attached bytes
                packet.RemoveRange(packetLength, packet.Count - packetLength);
                packet.RemoveRange(0, 4);

                // decrypt message
                byte[] decryptedPacket = AesEncrypterDecrypter.Decrypt(packet.ToArray(), AesKey, AesIV);

                return decryptedPacket;
            //}
            //catch (Exception ex) {
            //    Logger.Error(ex, "[ReceiveData]An error occured while receiving data.");
            //}

            //return null;
        }

        public async Task<byte[]> SendAndReceiveData(byte[] msg) {
            await LOCKER.WaitAsync();

            await SendData(msg);
            var received = await ReceiveData();

            LOCKER.Release();
            return received;
        }

        public async Task SendData(byte[] msg) {
            //try {
            Logger.Info($"[SendData] Sending data with length {msg.Length}.");
            List<byte> packet = new List<byte>();

            // encrypt message
            var encryptedMsg = AesEncrypterDecrypter.Encrypt(msg, AesKey, AesIV);

            // add length of packet - 4B
            packet.AddRange(BitConverter.GetBytes(encryptedMsg.Length + 4));

            // add content
            packet.AddRange(encryptedMsg);

            await networkStream.WriteAsync(packet.ToArray(), 0, packet.Count);
            await networkStream.FlushAsync();
            //return true;
            //}
            //catch (Exception ex) {
            //    Logger.Error(ex, "[SendData]An error occured while sending data.");
            //}

            //return false;
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
