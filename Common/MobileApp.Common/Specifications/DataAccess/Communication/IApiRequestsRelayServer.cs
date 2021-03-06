using System.Threading;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    /// <summary>
    /// Server that relays api requests threw an encrypted tunnel and that relays the answer back.
    /// This server is only locally accessable.
    /// </summary>
    public interface IApiRequestsRelayServer {

        /// <summary>
        /// Starts a local server that listens on the same port as the basestation API.
        /// </summary>
        /// <param name="relayTunnel">Tunnel to relay the received packages to.</param>
        /// <param name="cancellationToken">System.Threading.CancellationToken to stop the realy server.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a boolean that is true when the server got started successfully.
        /// </returns>
        Task<bool> Start(IEncryptedTunnel relayTunnel, CancellationToken cancellationToken);
    }
}
