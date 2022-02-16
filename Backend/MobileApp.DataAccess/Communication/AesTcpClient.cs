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

    /// <inheritdoc/>
    public class AesTcpClient : IAesTcpClient, IDisposable, IEncryptedTunnel {

        private static SemaphoreSlim LOCKER = new SemaphoreSlim(1);


        private TcpClient tcpClient;

        private NetworkStream networkStream;


        private ILogger Logger;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        public AesTcpClient(ILoggerService loggerService, IAesEncrypterDecrypter aesEncrypterDecrypter) {
            Logger = loggerService.GetLogger<AesTcpClient>();
            AesEncrypterDecrypter = aesEncrypterDecrypter;
        }

        public void Dispose() {
            networkStream?.Close();
            tcpClient?.Close();
        }

        /// <inheritdoc/>
        public bool IsConnected() {
            return tcpClient?.Connected ?? false;
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReceiveData(CancellationToken cancellationToken = default) {
            Logger.Info($"[ReceiveData]Waiting to receive data from {tcpClient.Client.RemoteEndPoint.ToString()}.");

            byte[] packet = await CommunicationUtils.ReceiveAsync(Logger, networkStream, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) {
                return null;
            }

            // decrypt message
            byte[] decryptedPacket = AesEncrypterDecrypter.Decrypt(packet);

            return decryptedPacket;
        }

        /// <inheritdoc/>
        public async Task<byte[]> SendAndReceiveData(byte[] data, CancellationToken cancellationToken = default) {
            await LOCKER.WaitAsync();

            await SendData(data, cancellationToken);
            var received = await ReceiveData(cancellationToken);

            LOCKER.Release();
            return received;
        }

        /// <inheritdoc/>
        public async Task SendData(byte[] data, CancellationToken cancellationToken = default) {
            Logger.Info($"[SendData] Sending data with length {data.Length}.");

            // encrypt message
            byte[] encryptedPacket = AesEncrypterDecrypter.Encrypt(data);

            await CommunicationUtils.SendAsync(Logger, encryptedPacket, networkStream, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> Start(IPEndPoint remoteEndPoint, int receiveTimeout) {
            try {
                tcpClient = new TcpClient();
                tcpClient.SendTimeout = 1000; // 1s
                tcpClient.ReceiveTimeout = receiveTimeout;
                tcpClient.Client.Blocking = true;
                //await tcpClient.ConnectAsync(remoteEndPoint.Address, remoteEndPoint.Port);
                CommunicationUtils.ConnectWithTimout(tcpClient.Client, remoteEndPoint, millisecondsTimeout: 2000);
                networkStream = tcpClient.GetStream();

                return true;
            }
            catch (ArgumentNullException anex) {
                if (anex.ParamName == nameof(remoteEndPoint)) {
                    Logger.Error($"[Start]Remote end point was null.");
                }
                else {
                    Logger.Error(anex, $"[Start]Error while connecting to remote end point.");
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "[Start]Error while connecting to remote end point.");
            }

            return false;
        }

        /// <inheritdoc/>
        public void Stop() {
            if (IsConnected()) {
                networkStream?.Close();
                tcpClient?.Close();
            }
        }
    }
}
