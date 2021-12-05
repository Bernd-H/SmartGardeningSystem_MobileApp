using System.Net;

namespace MobileApp.Common.Specifications.DataObjects {
    public interface IRelayRequestResult {

        bool BasestationNotReachable { get; set; }

        IPEndPoint BasestaionEndPoint { get; set; }
    }
}
