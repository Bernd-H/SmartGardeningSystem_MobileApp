using System;

namespace MobileApp.Common.Specifications.DataObjects {
    /// <summary>
    /// Contains information of what currently active connection should be used in LocalRelayManager in the project "GardeningSystem" to forward data.
    /// </summary>
    public interface IServicePackage {

        Guid SessionId { get; set; }

        byte[] Data { get; set; }
    }
}
