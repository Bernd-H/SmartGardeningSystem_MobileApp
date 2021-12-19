using System;
using NLog;

namespace MobileApp.Common.Specifications {
    public interface ILoggerService {
        ILogger GetLogger<T>() where T : class;

        string GetLogFilePath(bool allLogsFile);

        string GetInternalLogFilePath();

        /// <summary>
        /// The <paramref name="eventHandler"/> will get invoked when a new entry gets written to the log file.
        /// </summary>
        /// <param name="eventHandler"></param>
        void AddEventHandler(EventHandler eventHandler);

        bool RemoveEventHandler(EventHandler eventHandler);
    }
}
