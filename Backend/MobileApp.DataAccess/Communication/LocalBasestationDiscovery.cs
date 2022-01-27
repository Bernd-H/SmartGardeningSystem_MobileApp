using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class LocalBasestationDiscovery : ILocalBasestationDiscovery, IDisposable {

        /// <summary>
        /// Maximum time to await an answer after the message to the multicast group got sent.
        /// (Set to 5 seconds)
        /// </summary>
        static TimeSpan ReceiveTimeOut = new TimeSpan(0, 0, 5);

        private IMulticastUdpSender MulticastUdpSender;

        private ILogger Logger;

        private UdpClient client;

        public LocalBasestationDiscovery(IMulticastUdpSender multicastUdpSender, ILoggerService loggerService) {
            Logger = loggerService.GetLogger<LocalBasestationDiscovery>();
            MulticastUdpSender = multicastUdpSender;

            // setup udp listener, which listens for an answer from the basestation
            client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
        }

        public void Dispose() {
            client?.Dispose();
            MulticastUdpSender?.Dispose();
        }

        public async Task<BasestationFoundDto> TryFindBasestation() {
            BasestationFoundDto result = null;

            // start listening
            var asyncResult = client.BeginReceive(null, null);

            // send message
            int port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            await MulticastUdpSender.SendToMulticastGroupAsync(GetLocalIPAddress(), port);

            // wait for an answer
            asyncResult.AsyncWaitHandle.WaitOne(ReceiveTimeOut);
            if (asyncResult.IsCompleted) {
                try {
                    IPEndPoint remoteEP = null;
                    var receivedData = client.EndReceive(asyncResult, ref remoteEP);

                    Logger.Info($"[TryFindBasestation]Received an answer from {remoteEP}.");

                    // parse data to dto
                    result = new BasestationFoundDto() {
                        Id = new Guid(receivedData),
                        RemoteEndPoint = remoteEP
                    };
                }
                catch (Exception ex) {
                    Logger.Error(ex, "[TryFindBasestation]EndReceive or parsing data failed.");
                }
            }
            else {
                // The operation wasn't completed before the timeout
                Logger.Trace($"[TryFindBasestation]Udp receive timed out.");
            }

            return result;
        }

        static IPAddress GetLocalIPAddress() {
            string localIP = string.Empty;
            try {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
            }
            catch {
                localIP = string.Empty;
            }

            IPAddress ip;
            if (IPAddress.TryParse(localIP, out ip)) {
                return ip;
            }
            else {
                throw new Exception("[GetLocalIPAddress]Error while trying to identify the local ip address.");
            }
        }

        [Obsolete]
        static IPAddress GetLocalIPAddress_old() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip;
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
