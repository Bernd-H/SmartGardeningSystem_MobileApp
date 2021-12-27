using System.Net;

namespace MobileApp.Common.Models.Entities {
    public class ConnectRequestResult {

        public bool BasestationNotReachable { get; set; }

        public IPEndPoint BasestaionEndPoint { get; set; }
    }
}
