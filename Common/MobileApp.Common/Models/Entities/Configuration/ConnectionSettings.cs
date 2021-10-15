using MobileApp.Common.Specifications.Configuration;

namespace MobileApp.Common.Models.Entities.Configuration {
    public class ConnectionSettings : IConnectionSettings {
        public int LocalPeerDiscovery_port { get; }

        public string LocalPeerDiscovery_multicastIP { get; }
    }
}
