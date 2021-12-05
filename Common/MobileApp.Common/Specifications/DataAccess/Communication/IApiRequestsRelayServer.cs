﻿using System.Threading;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IApiRequestsRelayServer {

        /// <summary>
        /// Starts a local server that listens on the same port as the basestation API
        /// </summary>
        /// <param name="relayTunnel">Tunnel to relay the received packages to.</param>
        /// <param name="cancellationToken">To stop the server.</param>
        /// <returns></returns>
        Task<bool> Start(IEncryptedTunnel relayTunnel, CancellationToken cancellationToken);
    }
}
