namespace MobileApp.Common.Specifications.DataObjects {
    public enum ServiceType {
        API = 0,
        AesTcp = 1
    }

    /// <summary>
    /// Used in WanPackages, when the user accesses this server via peer to peer or via the external server.
    /// </summary>
    public interface IServiceDetails {

        int Port { get; set; }

        ServiceType Type { get; set; }

        /// <summary>
        /// True, to hold the connection from the LocalRelayManager in the Project GardeningSystem to the specified Service (ServiceType, Port) in Gardeningsystem open
        /// </summary>
        bool HoldConnectionOpen { get; set; }
    }
}
