using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess {

    public interface ISecureStorage {

        Task<string> Read(string key);

        Task<bool> Write(string key, string value);

        bool Remove(string key);
    }
}
