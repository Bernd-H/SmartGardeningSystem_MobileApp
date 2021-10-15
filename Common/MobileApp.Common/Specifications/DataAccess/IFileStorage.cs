using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess {
    public interface IFileStorage {
        Task<string> ReadAsString(string filename);
    }
}
