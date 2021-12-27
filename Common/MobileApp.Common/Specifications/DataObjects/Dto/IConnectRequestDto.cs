namespace MobileApp.Common.Specifications.DataObjects.Dto {
    public interface IConnectRequestDto {

        byte[] BasestationId { get; set; }

        bool ForceRelay { get; set; }
    }
}
