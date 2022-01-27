using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using MobileApp.Common.Specifications.DataAccess;

namespace MobileApp.DataAccess {
    public class FileStorageWin : IFileStorage {

        public Task<string> ReadAsString(string filePath) {
            var text = File.ReadAllText(filePath);
            return Task.FromResult(text);
        }

        public Task WriteAllText(string filePath, string text) {
            File.WriteAllText(filePath, text);

            return Task.CompletedTask;
        }
    }
}
