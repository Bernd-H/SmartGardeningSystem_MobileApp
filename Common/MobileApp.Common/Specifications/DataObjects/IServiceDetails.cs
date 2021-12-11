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
    }
}
