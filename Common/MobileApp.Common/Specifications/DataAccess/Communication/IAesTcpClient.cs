using System;
using System.Net;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IAesTcpClient : IDisposable {

        Task SendData(byte[] msg);

        Task<byte[]> ReceiveData();

        Task<bool> Start(IPEndPoint remoteEndPoint);

        void Stop();

        bool IsConnected();
    }
}
