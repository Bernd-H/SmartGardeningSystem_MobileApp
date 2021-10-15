using System;
using System.Collections.Generic;
using MobileApp.Common.Specifications;
using NLog;

namespace MobileApp.BusinessLogic {
    public class LoggerService : ILoggerService {

        private Dictionary<Type, ILogger> logger; // to allow to return new logger instances for different classes

        public LoggerService() {
            logger = new Dictionary<Type, ILogger>();
        }

        public ILogger GetLogger<T>() where T : class {
            if (!logger.ContainsKey(typeof(T))) {
                logger.Add(typeof(T), LogManager.GetLogger(typeof(T).Name));
            }

            return logger[typeof(T)];
        }
    }
}
