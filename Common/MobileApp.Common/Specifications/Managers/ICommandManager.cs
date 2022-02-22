using System;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;

namespace MobileApp.Common.Specifications.Managers {

    /// <summary>
    /// Class to send commands to the basestation.
    /// </summary>
    public interface ICommandManager : IDisposable {

        /// <summary>
        /// Trys to connect the basestation to another wifi.
        /// </summary>
        /// <param name="wlanInfo">Wlan-login-data.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> ConnectToWlan(WlanInfoDto wlanInfo);

        /// <summary>
        /// Disconnects the basestation from a wifi.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> DisconnectFromWlan();

        /// <summary>
        /// Enables the automatic irrigation algorithm of the basestation that get's activated automatically to specific times.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> StartAutomaticIrrigation();

        /// <summary>
        /// Disables the automatic irrigation algorithm.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> StopAutomaticIrrigation();

        /// <summary>
        /// Opens all valves which are enabled for manual irrigation immediately.
        /// </summary>
        /// <param name="timeSpan">Time the valves should stay open. The maximum timespan a valve can stay open is 8,5 hours.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> StartManualIrrigation(TimeSpan timeSpan);

        /// <summary>
        /// Closes all valves that are enabled for manual irrigation.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> StopManualIrrigation();

        /// <summary>
        /// Searches for a new module to connect to the system.
        /// This process can take some time.
        /// </summary>
        /// <param name="cancellationToken">System.Threading.CancellationToken to abort the search.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains the Id of the new module if a new module got connected successfully and is null if no new module was found.
        /// </returns>
        /// <remarks>More information about this new module can be requested through the APIManager.</remarks>
        /// <seealso cref="IAPIManager"/>
        Task<byte?> DiscoverNewModule(CancellationToken cancellationToken);

        /// <summary>
        /// Tests if the command manager service is available.
        /// (For debug reasons)
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains if the command services is available or not.
        /// </returns>
        Task<bool> Test();
    }
}
