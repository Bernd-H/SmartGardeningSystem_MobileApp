using System.Net.Security;
using MobileApp.Common.Specifications.DataAccess.Communication;

namespace MobileApp.Common.Specifications.Cryptography {
    public interface IAesTunnelInSslStream : IEncryptedTunnel {

        void Init(SslStream sslStream);
    }
}
