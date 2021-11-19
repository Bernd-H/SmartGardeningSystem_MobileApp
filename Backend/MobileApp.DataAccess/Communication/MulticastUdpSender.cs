using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class MulticastUdpSender : IMulticastUdpSender, IDisposable {

        /// <summary>
        /// The IPAddress and port of the IPV4 multicast group.
        /// </summary>
        static readonly IPEndPoint MulticastAddressV4 = new IPEndPoint(IPAddress.Parse("224.20.21.18"), 6771);

        /// <summary>
        /// String to search for in a message received from the multicast group, indicating that this message is for a
        /// gardening system.
        /// </summary>
        static readonly string GardeningSystemIdentificationString = "RRvIWZx4JTc0k7BoCvCG5A==";

        /// <summary>
        /// Used to generate a unique identifier for this client instance.
        /// </summary>
        static readonly Random Random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// When we send Announce we should embed the current <see cref="EngineSettings.ListenPort"/> as it is dynamic.
        /// </summary>
        string BaseSearchString { get; }

        /// <summary>
        /// A random identifier used to detect our own Announces so we can ignore them.
        /// </summary>
        string Cookie { get; }


        private ILogger Logger;

        private UdpClient sendClient;

        public MulticastUdpSender(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<MulticastUdpSender>();
            sendClient = new UdpClient();

            lock (Random)
                Cookie = $"1.0.0.0-{Random.Next(1, int.MaxValue)}";
            BaseSearchString = $"GS-SEARCH * HTTP/1.1 {GardeningSystemIdentificationString}\r\nHost: {MulticastAddressV4.Address}:{MulticastAddressV4.Port}\r\nIP: {{0}}\r\nPort: {{1}}\r\ncookie: {Cookie}\r\n\r\n\r\n";

        }

        public void Dispose() {
            sendClient?.Dispose();
        }

        public async Task SendToMulticastGroupAsync(IPAddress localIP, int replyPort) {
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            string message = string.Format(BaseSearchString, localIP.ToString(), replyPort);
            byte[] data = Encoding.ASCII.GetBytes(message);

            foreach (var nic in nics) {
                try {
                    sendClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.HostToNetworkOrder(nic.GetIPProperties().GetIPv4Properties().Index));
                    await sendClient.SendAsync(data, data.Length, MulticastAddressV4).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    Logger.Trace(ex, "[SendToMulticastGroupAsync]Error while sending data to multicast group.");
                }
            }
        }
    }
}
