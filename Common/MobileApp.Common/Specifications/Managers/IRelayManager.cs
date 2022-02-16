using System.Threading;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Managers {

    /// <summary>
    /// Class to establish a connection to the basestation over the internet and to start local services that relay all traffic through that connection.
    /// </summary>
    public interface IRelayManager {

        /// <summary>
        /// Tells the external server that we want to establish a connection to a specific basestation.
        /// Changes the endpoints of all clients to a local relay server, if the connection with the basestation was successful.
        /// </summary>
        /// <param name="cancellationToken">To close all open connections and stop relaying api requests or commands.</param>
        /// <param name="forceRelay">True, to redirect all traffic over the external server and not directly to the basestation.</param>
        /// <param name="test">True when the the connection is only for a relay test.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean that is true when a connection to the basestation got esablished.
        /// </returns>
        /// <remarks>
        /// Trys to establish a connection directly to the basestation.
        /// If that isn't possible a secure end-to-end encrypted tunnel will get opend over the external server to the basestation.
        /// </remarks>
        Task<bool> ConnectToTheBasestation(CancellationToken cancellationToken, bool forceRelay = false, bool test = false);
    }
}
