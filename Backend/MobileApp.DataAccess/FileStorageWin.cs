using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using MobileApp.Common.Specifications.DataAccess;

namespace MobileApp.DataAccess {

    /// <inheritdoc/>
    public class FileStorageWin : IFileStorage {

        /// <inheritdoc/>
        public Task<byte[]> Read(string filePath) {
            return Task.FromResult(File.ReadAllBytes(filePath));
        }

        /// <inheritdoc/>
        public Task<string> ReadAsString(string filePath) {
            var text = File.ReadAllText(filePath);
            return Task.FromResult(text);
        }

        /// <inheritdoc/>
        public Task Write(string filePath, byte[] data) {
            File.WriteAllBytes(filePath, data);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task WriteAllText(string filePath, string text) {
            File.WriteAllText(filePath, text);

            return Task.CompletedTask;
        }
    }
}
