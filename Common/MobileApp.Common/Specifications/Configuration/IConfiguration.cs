namespace MobileApp.Common.Specifications.Configuration {
    public interface IConfiguration {

        IConnectionSettings ConnectionSettings { get; }

        IFileNames FileNames { get; }
    }
}
