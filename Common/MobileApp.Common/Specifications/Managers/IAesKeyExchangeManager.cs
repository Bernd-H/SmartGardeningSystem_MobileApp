using System.Threading;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Managers {

    /// <summary>
    /// Class to get a aes key from a basestation.
    /// </summary>
    public interface IAesKeyExchangeManager {

        /// <summary>
        /// Connects to the basestation and receives the aes key through a secure connection.
        /// </summary>
        /// <param name="token">System.Threading.CancellationToken to stop the process.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a boolean that is true when the key got retrieved successfully.
        /// </returns>
        Task<bool> Start(CancellationToken token);
    }
}
