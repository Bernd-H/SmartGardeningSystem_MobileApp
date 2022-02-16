using System;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Managers {

    /// <summary>
    /// Class to locate a basestation in the local network.
    /// </summary>
    public interface IBasestationFinderManager : IDisposable {

        /// <summary>
        /// Trys to find a basestation via a multicast ip address.
        /// Retries a view times if no basestation answered.
        /// Stores the ip address of a found basestation in settings.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a boolean that is true when a basestation got found.
        /// </returns>
        Task<bool> FindLocalBaseStation();
    }
}
