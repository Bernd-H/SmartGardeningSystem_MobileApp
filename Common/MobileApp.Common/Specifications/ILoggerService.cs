using NLog;

namespace MobileApp.Common.Specifications {
    public interface ILoggerService {
        ILogger GetLogger<T>() where T : class;

        string GetLogFilePath(bool allLogsFile);

        string GetInternalLogFilePath();
    }
}
