using System;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Managers {
    public interface IBasestationFinderManager : IDisposable {

        /// <summary>
        /// Trys to find a basestation via a multicast ip address.
        /// Retries a view times if no basestation answered.
        /// Stores the ip address of a found basestation in settings.
        /// </summary>
        /// <returns>True when a basestation got found. Otherwise false.</returns>
        Task<bool> FindLocalBaseStation();
    }
}
