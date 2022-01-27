using System;
using MobileApp;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.DI;

namespace BasestationStressTest
{

    /// <summary>
    /// Sends and receives many concurrent packages over the internet to the basestation.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            DI.RegisterDependencies();

            var logger = IoC.Get<ILoggerService>().GetLogger<Program>();
            logger.Info($"[Main]Starting up test program...");



        }
    }
}
