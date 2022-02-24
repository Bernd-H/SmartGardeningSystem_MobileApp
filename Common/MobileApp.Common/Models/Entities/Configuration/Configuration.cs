using MobileApp.Common.Specifications.Configuration;

namespace MobileApp.Common.Models.Entities.Configuration {
    public class Configuration : IConfiguration {
        public ConnectionSettings ConnectionSettings { get; set; }

        public FileNames FileNames { get; set; }

        public int AesKeyLength_Bytes { get; set; }

        public int AesIvLength_Bytes { get; set; }
    }
}
