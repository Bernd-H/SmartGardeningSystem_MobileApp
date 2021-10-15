using MobileApp.Common.Specifications.Configuration;

namespace MobileApp.Common.Models.Entities.Configuration {
    public class Configuration : IConfiguration {
        public IConnectionSettings ConnectionSettings { get; }

        public IFileNames FileNames { get; }
    }
}
