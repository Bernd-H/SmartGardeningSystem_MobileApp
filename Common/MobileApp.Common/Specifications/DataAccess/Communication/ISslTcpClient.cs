using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    /// <summary>
    /// A Tcp client that sends and receives all packages over a Ssl stream.
    /// </summary>
    public interface ISslTcpClient : IEncryptedTunnel {

        /// <summary>
        /// Gets the Ssl stream. Null when the client isn't connected to the server.
        /// </summary>
        SslStream SslStream { get; }

        /// <summary>
        /// Builds the tcp connection to the <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="endPoint">IP end point of the server to connect to.</param>
        /// <param name="selfSignedCertificate">True to ignore the server certificate.</param>
        /// <param name="targetHost">The name of the server that shares this System.Net.Security.SslStream.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains a boolean that is true when everything went good.
        /// </returns>
        Task<bool> Start(IPEndPoint endPoint, bool selfSignedCertificate = true, string targetHost = "server");

        /// <summary>
        /// Gets the certificate of the server.
        /// </summary>
        /// <returns>A X509Certificate object. Null when the client isn't connected to the server.</returns>
        X509Certificate GetServerCert();

        /// <summary>
        /// Disconnects the tcp client form the server and closes the ssl stream.
        /// </summary>
        void Stop();
    }
}
