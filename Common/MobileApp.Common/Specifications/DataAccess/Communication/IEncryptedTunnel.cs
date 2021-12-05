using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IEncryptedTunnel {

        Task<byte[]> ReceiveData();

        Task SendData(byte[] msg);
    }
}
