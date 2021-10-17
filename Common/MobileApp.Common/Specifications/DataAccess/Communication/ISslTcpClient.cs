using System.Net;
using System.Net.Security;

namespace MobileApp.Common.Specifications.DataAccess.Communication {

    public delegate void SslStreamOpenCallback(SslStream openStream);

    public interface ISslTcpClient {

        bool RunClient(IPEndPoint endPoint, SslStreamOpenCallback sslStreamOpenCallback);
    }
}
