using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class SslTcpClient : ISslTcpClient {

        private bool _validateCertificate;


        private ILogger Logger;

        public SslTcpClient(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<SslTcpClient>();
        }

        public async Task<bool> RunClient(IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback, bool selfSignedCertificate = true,
            bool closeConnectionAfterCallback = true) {
            bool result = false;
            TcpClient client = null;
            SslStream sslStream = null;

            _validateCertificate = !selfSignedCertificate;

            try {
                client = new TcpClient();
                client.ReceiveTimeout = 1000; // 1s
                client.SendTimeout = 1000;
                client.Client.Blocking = true;
                await client.ConnectAsync(endPoint.Address, endPoint.Port);
                Logger.Info($"[RunClient]Connected to server {endPoint.ToString()}.");

                // Create an SSL stream that will close the client's stream.
                sslStream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null);

                sslStream.AuthenticateAsClient("server");

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

        public static byte[] ReadMessage(SslStream sslStream) {
            int bytes = -1;
            int packetLength = -1;
            int readBytes = 0;
            List<byte> packet = new List<byte>();

            do {
                byte[] buffer = new byte[2048];
                bytes = sslStream.Read(buffer, 0, buffer.Length);

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

            return packet.ToArray();
        }

        public static void SendMessage(SslStream sslStream, byte[] msg) {
            List<byte> packet = new List<byte>();

            // add length of packet - 4B
            packet.AddRange(BitConverter.GetBytes(msg.Length + 4));

            // add content
            packet.AddRange(msg);

            sslStream.Write(packet.ToArray());
            sslStream.Flush();
        }
    }
}
