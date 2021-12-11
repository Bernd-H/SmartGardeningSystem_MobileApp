using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IEncryptedTunnel {

        Task<byte[]> ReceiveData();

        Task SendData(byte[] msg);

        /// <summary>
        /// Calls first SendData() and then ReceiveData().
        /// For multithreading (locking) purposes.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task<byte[]> SendAndReceiveData(byte[] msg);
    }
}
