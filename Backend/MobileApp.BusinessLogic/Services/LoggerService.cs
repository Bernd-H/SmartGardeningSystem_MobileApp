using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace MobileApp.BusinessLogic {
    public class LoggerService : ILoggerService {

        private Dictionary<Type, ILogger> logger; // to allow to return new logger instances for different classes

        public LoggerService() {
            logger = new Dictionary<Type, ILogger>();
            LoadConfig();
        }

        public ILogger GetLogger<T>() where T : class {
            if (!logger.ContainsKey(typeof(T))) {
                logger.Add(typeof(T), LogManager.GetLogger(typeof(T).Name));
            }

            return logger[typeof(T)];
        }

        public string GetLogFilePath(bool allLogsFile) {
            LogEventInfo logEventInfo;

            // to set the timestamp and folderpath on Render()
            // (Render replaces ${date:format=yyyy-MM-dd} with the current timestamp and ${specialfolder:folder=MyDocuments} with the correct folder path)
            logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };

            if (allLogsFile) {
                return clearFileNamePathFromApostrophs(((FileTarget)LogManager.Configuration.FindTargetByName("allLogFile")).FileName.Render(logEventInfo));
            } else {
                return clearFileNamePathFromApostrophs(((FileTarget)LogManager.Configuration.FindTargetByName("logFile")).FileName.Render(logEventInfo));
            }
        }

        public string GetInternalLogFilePath() {
            //LogManager.Configuration.
            throw new NotImplementedException();
        }

        private static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceFileName) {
            var resourcePaths = assembly.GetManifestResourceNames()
              .Where(x => x.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase))
              .ToList();
            if (resourcePaths.Count == 1) {
                return assembly.GetManifestResourceStream(resourcePaths.Single());
            }
            return null;
        }

        private static void LoadConfig() {
            // load config file
            var assembly = typeof(IoC).GetTypeInfo().Assembly; // MobileApp.Common assembly
            var nlogConfigFile = GetEmbeddedResourceStream(assembly, "NLog.config");
            if (nlogConfigFile != null) {
                var xmlReader = System.Xml.XmlReader.Create(nlogConfigFile);
                NLog.LogManager.Configuration = new XmlLoggingConfiguration(xmlReader, null);
            }

            // set log file paths
            // all log file paths get already correctly set in the NLog.config file
            //var allLogFileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("allLogFile");
            //var fileName = clearFileNamePathFromApostrophs(allLogFileTarget.FileName.ToString());
            //allLogFileTarget.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            //var ownLogFileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("logFile");
            //fileName = clearFileNamePathFromApostrophs(ownLogFileTarget.FileName.ToString());
            //ownLogFileTarget.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            //var errorLogFileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("errorLogFile");
            //fileName = clearFileNamePathFromApostrophs(errorLogFileTarget.FileName.ToString());
            //errorLogFileTarget.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            //LogManager.ReconfigExistingLoggers();
        }

        private static string clearFileNamePathFromApostrophs(string fileNameOrPath) {
            return fileNameOrPath.Replace("'", string.Empty);
        }

        public void AddEventHandler(EventHandler eventHandler) {
            LoggingEventTarget.NLogFileChangedEventHandlers.Remove(eventHandler);
            LoggingEventTarget.NLogFileChangedEventHandlers.Add(eventHandler);
        }

        public bool RemoveEventHandler(EventHandler eventHandler) {
            return LoggingEventTarget.NLogFileChangedEventHandlers.Remove(eventHandler);
        }

        public static void AddCustomLogTargets() {
            Target.Register<LoggingEventTarget>("LoggingEvent");
        }
    }

    [Target("LoggingEvent")]
    public sealed class LoggingEventTarget : Target {

        public static List<EventHandler> NLogFileChangedEventHandlers = new List<EventHandler>();

        public LoggingEventTarget() {

        }

        protected override void Write(LogEventInfo logEvent) {
            // raise events
            foreach (var eventHandler in NLogFileChangedEventHandlers) {
                eventHandler?.Invoke(this, null);
            }
        }
    }
}
