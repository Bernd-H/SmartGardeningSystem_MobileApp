using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    public delegate void SslStreamOpenCallback(SslStream openStream, X509Certificate x509Certificate);

    public interface ISslTcpClient {

        Task<bool> RunClient(IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback, bool selfSignedCertificate = true, bool closeConnectionAfterCallback = true,
            string targetHost = "server");

        byte[] ReadMessage(SslStream sslStream);

        void SendMessage(SslStream sslStream, byte[] msg);
    }
}
