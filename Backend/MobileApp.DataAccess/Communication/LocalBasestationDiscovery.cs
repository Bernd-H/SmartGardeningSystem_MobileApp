using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class LocalBasestationDiscovery : ILocalBasestationDiscovery, IDisposable {

        /// <summary>
        /// Maximum time to await an answer after the message to the multicast group got sent.
        /// (Set to 3 seconds)
        /// </summary>
        static TimeSpan ReceiveTimeOut = new TimeSpan(0, 0, 3);

        private IMulticastUdpSender MulticastUdpSender;

        private ILogger Logger;

        private UdpClient listener;

        public LocalBasestationDiscovery(IMulticastUdpSender multicastUdpSender, ILoggerService loggerService) {
            Logger = loggerService.GetLogger<LocalBasestationDiscovery>();
            MulticastUdpSender = multicastUdpSender;

            // setup udp listener, which listens for an answer from the basestation
            listener = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
        }

        public void Dispose() {
            listener?.Dispose();
            MulticastUdpSender?.Dispose();
        }

        public async Task<BasestationFoundDto> TryFindBasestation() {
            BasestationFoundDto result = null;

            // send message
            int port = ((IPEndPoint)listener.Client.LocalEndPoint).Port;
            await MulticastUdpSender.SendToMulticastGroupAsync(port);

            // wait for an answer
            var asyncResult = listener.BeginReceive(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(ReceiveTimeOut);
            if (asyncResult.IsCompleted) {
                try {
                    IPEndPoint remoteEP = null;
                    var receivedData = listener.EndReceive(asyncResult, ref remoteEP);

                    // parse data to dto
                    result = new BasestationFoundDto() {
                        Id = Guid.Parse(Encoding.ASCII.GetString(receivedData)),
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
    }
}
