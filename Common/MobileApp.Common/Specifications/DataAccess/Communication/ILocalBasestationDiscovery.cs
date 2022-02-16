using System;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    /// <summary>
    /// Class that commissions the multicast UDP packages and handles the answer of a basestation.
    /// </summary>
    public interface ILocalBasestationDiscovery : IDisposable {

        /// <summary>
        /// Tries to find a basestation in the local area through sending a multicast message.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a BasestationFoundDto object containing the Id and remote end point of the basestation.
        /// </returns>
        Task<BasestationFoundDto> TryFindBasestation();
    }
}
