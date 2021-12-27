using MobileApp.Common.Specifications.DataObjects.Dto;

namespace MobileApp.Common.Models.DTOs {
    public class ConnectRequestDto : IConnectRequestDto {

        public byte[] BasestationId { get; set; }

        public bool ForceRelay { get; set; }
    }
}
