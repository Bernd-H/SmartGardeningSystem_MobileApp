using System.Threading;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Managers {
    public interface IRelayManager {

        /// <summary>
        /// Tells the external server to connect it to the basestation.
        /// Changes the endpoints of all clients to a local relay server, if the connection with the basestation was successful.
        /// </summary>
        /// <param name="cancellationToken">To close all open connections and stop relaying api requests or commands.</param>
        /// <returns>True, when a connection got esablished.</returns>
        Task<bool> ConnectToTheBasestation(CancellationToken cancellationToken);
    }
}
