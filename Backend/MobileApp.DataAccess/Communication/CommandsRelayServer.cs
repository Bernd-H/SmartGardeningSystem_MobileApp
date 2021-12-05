using System;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Specifications.DataAccess.Communication;

namespace MobileApp.DataAccess.Communication {
    public class CommandsRelayServer : ICommandsRelayServer {
        public Task<bool> Start(IEncryptedTunnel relayTunnel, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }
    }
}
