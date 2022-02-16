using System;
using System.Net;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    /// <summary>
    /// Tcp client that sends all packages aes encrypted and decryptes all received ones.
    /// </summary>
    public interface IAesTcpClient : IDisposable, IEncryptedTunnel {

        /// <summary>
        /// Builds the tcp connection to the <paramref name="remoteEndPoint"/>.
        /// </summary>
        /// <param name="remoteEndPoint">IP end point of the server to connect to.</param>
        /// <param name="receiveTimeout">Sets the amount of time a System.Net.Sockets.TcpClient will wait to receive data once a read operation is initiated.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains a boolean that is true when a connection got establilshed successfully.
        /// </returns>
        Task<bool> Start(IPEndPoint remoteEndPoint, int receiveTimeout);

        /// <summary>
        /// Disconnects the tcp client form the server.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets a value indicating whether the underlying System.Net.Sockets.Socket for a System.Net.Sockets.TcpClient is connected to a remote host.
        /// </summary>
        /// <returns>True if the System.Net.Sockets.TcpClient.Client socket was connected to a remote resource as of the most recent operation; otherwise, false.</returns>
        bool IsConnected();
    }
}
