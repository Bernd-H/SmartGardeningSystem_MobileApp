using System;
using NLog;

namespace MobileApp.Common.Specifications {

    /// <summary>
    /// Class to get an NLog logger for a specific class.
    /// </summary>
    public interface ILoggerService {

        /// <summary>
        /// Gets a NLog logger instance.
        /// </summary>
        /// <typeparam name="T">Type of the class the logger is for.</typeparam>
        /// <returns>Logger for class with type <typeparamref name="T"/>.</returns>
        ILogger GetLogger<T>() where T : class;

        /// <summary>
        /// Gets the path of the log file.
        /// </summary>
        /// <param name="allLogsFile">True to get the path of the log file where all logs get stored in (including TRACE logs).</param>
        /// <returns>Full filepath.</returns>
        string GetLogFilePath(bool allLogsFile);

        /// <summary>
        /// Adds a new eventhandler.
        /// The <paramref name="eventHandler"/> will get invoked when a new entry gets written to the log file.
        /// </summary>
        /// <param name="eventHandler">Eventhandler to add.</param>
        void AddEventHandler(EventHandler eventHandler);

        /// <summary>
        /// Removes an existing eventhandler.
        /// </summary>
        /// <param name="eventHandler">Eventhandler to remove.</param>
        /// <returns>True if the event handler got removed successfully.</returns>
        bool RemoveEventHandler(EventHandler eventHandler);
    }
}
