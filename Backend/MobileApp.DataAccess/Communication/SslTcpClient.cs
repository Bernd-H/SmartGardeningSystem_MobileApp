using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Utilities;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class SslTcpClient : ISslTcpClient {

        private bool _validateCertificate;


        private ILogger Logger;

        public SslTcpClient(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<SslTcpClient>();
        }

        public async Task<bool> RunClient(IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback, bool selfSignedCertificate = true,
            bool closeConnectionAfterCallback = true, string targetHost = "server") {
            bool result = false;
            TcpClient client = null;
            SslStream sslStream = null;

            _validateCertificate = !selfSignedCertificate;

            try {
                client = new TcpClient();
                client.ReceiveTimeout = 20000; // 20s
                client.SendTimeout = 5000;
                client.Client.Blocking = true;
                await client.ConnectAsync(endPoint.Address, endPoint.Port);
                Logger.Info($"[RunClient]Connected to server {endPoint.ToString()}.");

                // Create an SSL stream that will close the client's stream.
                sslStream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null);

                //sslStream.ReadTimeout = 5000;
                sslStream.ReadTimeout = 20000;
                sslStream.WriteTimeout = 5000;

                sslStream.AuthenticateAsClient(targetHost);

                sslStreamOpenCallback.Invoke(sslStream);
                result = true;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[RunClient]An exception occured.");
                sslStream?.Close();
                client?.Close();
            }
            finally {
                if (closeConnectionAfterCallback) {
                    sslStream?.Close();
                }
            }

            return result;
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

        public byte[] ReadMessage(SslStream sslStream) {
            return CommunicationUtils.Receive(Logger, sslStream);
        }

        public void SendMessage(SslStream sslStream, byte[] msg) {
            CommunicationUtils.Send(Logger, msg, sslStream);
        }
    }
}
