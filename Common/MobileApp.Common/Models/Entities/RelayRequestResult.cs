using System.Net;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.Entities {
    public class RelayRequestResult : IRelayRequestResult {

        public bool BasestationNotReachable { get; set; }

        public IPEndPoint BasestaionEndPoint { get; set; }
    }
}
