using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.Common.Specifications.DataAccess;

namespace MobileApp.DataAccess {
    public class FileStorageiOS : IFileStorage {

        public async Task<string> ReadAsString(string filePath) {
            var data = await ReadAsBytes(filePath);

            if (data == null)
                return string.Empty;

            return System.Text.Encoding.UTF8.GetString(data);
        }

        public async Task WriteAllText(string filePath, string text) {
            var data = System.Text.Encoding.UTF8.GetBytes(text);
            await WriteAsBytes(filePath, data);   
        }

        private Task WriteAsBytes(string filePath, byte[] data) {
            return Task.Run(() => {
                var bytes = new List<byte>(data);

                // add byte order mark
                var bom = new byte[] { 0xEF, 0xBB, 0xBF };
                bytes.InsertRange(0, bom);

                File.WriteAllBytes(filePath, bytes.ToArray());
            });
        }

        private Task<byte[]> ReadAsBytes(string filePath) {
            var data = File.ReadAllBytes(filePath);

            if (data != null)
                data = FileStorageiOS.CleanByteOrderMark(data);

            return Task.FromResult(data);
        }

        private static byte[] CleanByteOrderMark(byte[] bytes) {
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var empty = Enumerable.Empty<byte>();
            if (bytes.Take(3).SequenceEqual(bom))
                return bytes.Skip(3).ToArray();

            return bytes;
        }
    }
}
