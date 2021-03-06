using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using NLog;

namespace MobileApp.DataAccess.Communication {

    /// <inheritdoc/>
    public class MulticastUdpSender : IMulticastUdpSender, IDisposable {

        /// <summary>
        /// The IPAddress and port of the IPV4 multicast group.
        /// </summary>
        static readonly IPEndPoint MulticastAddressV4 = new IPEndPoint(IPAddress.Parse("239.192.20.21"), 6777);

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
        private string BaseSearchString;

        private UdpClient sendClient;


        private ILogger Logger;

        private ISettingsManager SettingsManager;

        public MulticastUdpSender(ILoggerService loggerService, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<MulticastUdpSender>();
            SettingsManager = settingsManager;
            sendClient = new UdpClient();
        }

        /// <inheritdoc/>
        public void Dispose() {
            sendClient?.Dispose();
        }

        /// <inheritdoc/>
        public async Task SendToMulticastGroupAsync(IPAddress localIP, int replyPort) {
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            await buildBaseSearchString();

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

        /// <summary>
        /// Builds the base search string for the local basestation discovery.
        /// The basestationId can also be Guid.Empty. In this case all basestations in the network will answer.
        /// </summary>
        /// <returns></returns>
        private async Task buildBaseSearchString() {
            var settigns = await SettingsManager.GetApplicationSettings();
            var basestationGuid = settigns.BasestationId;
            BaseSearchString = $"GS-SEARCH * HTTP/1.1 {GardeningSystemIdentificationString} {basestationGuid}\r\nHost: {MulticastAddressV4.Address}:{MulticastAddressV4.Port}" +
                $"\r\nIP: {{0}}\r\nPort: {{1}}\r\n\r\n\r\n";
        }
    }
}
