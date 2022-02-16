using System.Net.Security;
using MobileApp.Common.Specifications.DataAccess.Communication;

namespace MobileApp.Common.Specifications.Cryptography {

    /// <summary>
    /// Class that manages receive and send methods and adds a aes encryption layer.
    /// </summary>
    public interface IAesTunnelInSslStream : IEncryptedTunnel {

        /// <summary>
        /// Initializes the class.
        /// </summary>
        /// <param name="sslStream">Stream to send data and to receive form.</param>
        void Init(SslStream sslStream);
    }
}
