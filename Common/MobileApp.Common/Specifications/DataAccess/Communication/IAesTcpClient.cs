using System;
using System.Net;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IAesTcpClient : IDisposable, IEncryptedTunnel {

        // IEncryptedTunnel contains the following methods:
        //Task SendData(byte[] msg);
        //Task<byte[]> ReceiveData();

        Task<bool> Start(IPEndPoint remoteEndPoint, int receiveTimeout);

        void Stop();

        bool IsConnected();
    }
}
