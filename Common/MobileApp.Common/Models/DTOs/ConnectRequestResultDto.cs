using MobileApp.Common.Specifications.DataObjects.Dto;

namespace MobileApp.Common.Models.DTOs {
    public class ConnectRequestResultDto : IConnectRequestResultDto {
        public bool BasestationNotReachable { get; set; }

        public string BasestaionEndPoint { get; set; }
    }
}
