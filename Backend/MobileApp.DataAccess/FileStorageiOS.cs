using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.Common.Specifications.DataAccess;

namespace MobileApp.DataAccess {

    /// <inheritdoc/>
    public class FileStorageiOS : IFileStorage {

        /// <inheritdoc/>
        public async Task<string> ReadAsString(string filePath) {
            var data = await readAsBytes(filePath);

            if (data == null)
                return string.Empty;

            return System.Text.Encoding.UTF8.GetString(data);
        }

        /// <inheritdoc/>
        public async Task WriteAllText(string filePath, string text) {
            var data = System.Text.Encoding.UTF8.GetBytes(text);
            await writeAsBytes(filePath, data);   
        }

        /// <inheritdoc/>
        public Task<byte[]> Read(string filePath) {
            return readAsBytes(filePath);
        }

        /// <inheritdoc/>
        public Task Write(string filePath, byte[] data) {
            return writeAsBytes(filePath, data);
        }

        private Task writeAsBytes(string filePath, byte[] data) {
            var bytes = new List<byte>(data);

            // add byte order mark
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            bytes.InsertRange(0, bom);

            File.WriteAllBytes(filePath, bytes.ToArray());

            return Task.CompletedTask;
        }

        private Task<byte[]> readAsBytes(string filePath) {
            var data = File.ReadAllBytes(filePath);

            if (data != null)
                data = FileStorageiOS.cleanByteOrderMark(data);

            return Task.FromResult(data);
        }

        private static byte[] cleanByteOrderMark(byte[] bytes) {
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var empty = Enumerable.Empty<byte>();
            if (bytes.Take(3).SequenceEqual(bom))
                return bytes.Skip(3).ToArray();

            return bytes;
        }
    }
}
