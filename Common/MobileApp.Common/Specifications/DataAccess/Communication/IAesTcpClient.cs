using System;
using System.Net;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IAesTcpClient : IDisposable {

        Task<bool> SendData(byte[] msg);

        Task<byte[]> ReceiveData();

        Task<bool> Start(IPEndPoint remoteEndPoint);

        bool IsConnected();
    }
}
