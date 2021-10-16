using MobileApp.Common.Models.Entities.Configuration;

namespace MobileApp.Common.Specifications.Configuration {
    public interface IConfiguration {

        ConnectionSettings ConnectionSettings { get; set; }

        FileNames FileNames { get; set; }
    }
}
