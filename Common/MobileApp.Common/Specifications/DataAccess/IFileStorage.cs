using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess {
    public interface IFileStorage {
        Task<string> ReadAsString(string filePath);

        Task WriteAllText(string filePath, string text);
    }
}
