using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Utilities;
using NLog;

namespace MobileApp.DataAccess.Communication {

    /// <inheritdoc/>
    public class SslTcpClient : ISslTcpClient {

        public SslStream SslStream { get; private set; }

        private bool _validateCertificate;

        private SemaphoreSlim _locker = new SemaphoreSlim(1);

        private Socket _client;

        private ILogger Logger;

        public SslTcpClient(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<SslTcpClient>();
        }

        /// <inheritdoc/>
        public async Task<bool> Start(IPEndPoint endPoint, bool selfSignedCertificate = true, string targetHost = "server") {
            bool result = false;
            _validateCertificate = !selfSignedCertificate;

            try {
                _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _client.ReceiveTimeout = 20000; // 20s
                _client.SendTimeout = 5000;
                _client.Blocking = true;
                await _client.ConnectAsync(endPoint.Address, endPoint.Port);
                Logger.Info($"[RunClient]Connected to server {endPoint.ToString()}.");

                // Create an SSL stream that will close the client's stream.
                SslStream = new SslStream(
                    new NetworkStream(_client),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null);

                SslStream.ReadTimeout = 20000;
                SslStream.WriteTimeout = 5000;

                SslStream.AuthenticateAsClient(targetHost);

                result = true;
            }
            catch (SocketException) {
                Logger.Error($"[RunClient]Could not connect to {endPoint}.");
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[RunClient]An exception occured (ep={endPoint.ToString()}).");
                SslStream?.Close();
                _client?.Close();
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReceiveData(CancellationToken cancellationToken = default) {
            Logger.Info($"[ReceiveData]Waiting to receive data from {_client.RemoteEndPoint.ToString()}.");
            return await CommunicationUtils.ReceiveAsync(Logger, SslStream, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<byte[]> SendAndReceiveData(byte[] data, CancellationToken cancellationToken = default) {
            await _locker.WaitAsync();

            await SendData(data, cancellationToken);
            var received = await ReceiveData(cancellationToken);

            _locker.Release();
            return received;
        }

        /// <inheritdoc/>
        public async Task SendData(byte[] data, CancellationToken cancellationToken = default) {
            Logger.Info($"[SendData] Sending data with length {data.Length}.");
            await CommunicationUtils.SendAsync(Logger, data, SslStream, cancellationToken);
        }

        /// <inheritdoc/>
        public X509Certificate GetServerCert() {
            return SslStream?.RemoteCertificate;
        }

        /// <inheritdoc/>
        public void Stop() {
            SslStream?.Close();
            _client?.Close();
        }

        private bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors) {

            if (_validateCertificate) {
                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;

                return false;
            }
            else {
                // because it's a self signed certificate
                return true;
            }
        }
    }
}
