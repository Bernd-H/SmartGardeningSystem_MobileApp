namespace MobileApp.Common.Specifications.Configuration {
    public interface IConnectionSettings {

        int LocalPeerDiscovery_port { get; }

        string LocalPeerDiscovery_multicastIP { get; }
    }
}
