using System.Threading;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    /// <summary>
    /// Interface that defines send and receive methods 
    /// </summary>
    public interface IEncryptedTunnel {

        /// <summary>
        /// Reads data from an encrypted stream.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains the received decrypted data.
        /// </returns>
        Task<byte[]> ReceiveData(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes data encrypted to a stream.
        /// </summary>
        /// <param name="data">Data to send (plaintext).</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        Task SendData(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calls first SendData() and then ReceiveData().
        /// For multithreading (locking) purposes.
        /// </summary>
        /// <param name="data">Data to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains the received decrypted data.
        /// </returns>
        Task<byte[]> SendAndReceiveData(byte[] data, CancellationToken cancellationToken = default);
    }
}
