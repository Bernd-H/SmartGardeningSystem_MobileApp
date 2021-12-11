using System.IO;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess {
    public interface ISftpClient {

        Task<bool> UploadFile(string host, string username, string password, Stream input, string filepath, Stream privateKey);
    }
}
