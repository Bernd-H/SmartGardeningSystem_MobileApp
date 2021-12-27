using System.Net;

namespace MobileApp.Common.Specifications.DataObjects.Dto {
    public interface IConnectRequestResultDto {

        bool BasestationNotReachable { get; set; }

        string BasestaionEndPoint { get; set; }
    }
}
