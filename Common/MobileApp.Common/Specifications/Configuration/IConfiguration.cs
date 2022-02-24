using MobileApp.Common.Models.Entities.Configuration;

namespace MobileApp.Common.Specifications.Configuration {
    public interface IConfiguration {

        ConnectionSettings ConnectionSettings { get; set; }

        FileNames FileNames { get; set; }

        int AesKeyLength_Bytes { get; set; }

        int AesIvLength_Bytes { get; set; }
    }
}
