using System;
using System.Net;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    /// <summary>
    /// Class to build and send the multicast basestation search message.
    /// </summary>
    public interface IMulticastUdpSender : IDisposable {

        /// <summary>
        /// Builds the basestation search message and sends it to all internet interfaces.
        /// </summary>
        /// <param name="localIP">The private ip address of this device.</param>
        /// <param name="replyPort">Port the basestation should send the answer to.</param>
        /// <returns>A Task that represents an asynchronous operation.</returns>
        Task SendToMulticastGroupAsync(IPAddress localIP, int replyPort);
    }
}
