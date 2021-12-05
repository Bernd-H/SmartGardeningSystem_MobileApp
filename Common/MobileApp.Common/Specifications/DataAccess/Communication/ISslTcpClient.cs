using System.Net;
using System.Net.Security;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    public delegate void SslStreamOpenCallback(SslStream openStream);

    public interface ISslTcpClient {

        Task<bool> RunClient(IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback, bool selfSignedCertificate = true, bool closeConnectionAfterCallback = true);
    }
}
