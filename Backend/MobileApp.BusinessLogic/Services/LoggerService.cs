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
            var allLogFileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("allLogFile");
            allLogFileTarget.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), allLogFileTarget.FileName.ToString());

            var ownLogFileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("ownLogFile");
            ownLogFileTarget.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ownLogFileTarget.FileName.ToString());

            var errorLogFileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("errorLogFile");
            errorLogFileTarget.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), errorLogFileTarget.FileName.ToString());

            LogManager.ReconfigExistingLoggers();
        }
    }
}
