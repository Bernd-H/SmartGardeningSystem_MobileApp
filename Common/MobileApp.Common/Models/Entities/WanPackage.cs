using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.Entities {
    public class WanPackage : IWanPackage {

        public PackageType PackageType { get; set; }

        public byte[] Package { get; set; }

        public ServiceDetails ServiceDetails { get; set; }
    }
}
