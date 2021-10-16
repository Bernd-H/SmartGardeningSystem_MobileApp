using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using MobileApp.Common.Specifications.DataAccess;

namespace MobileApp.DataAccess {
    public class FileStorageAndroid : IFileStorage {

        private Context _context = Application.Context;

        public Task<string> ReadAsString(string filePath) {
            return Task.Run(() => {
                return File.ReadAllText(filePath);
            });
        }

        //public async Task<string> ReadAsString(string filename) {
        //    using (var asset = _context.Assets.Open(filename)) {
        //        using (var streamReader = new StreamReader(asset)) {
        //            return await streamReader.ReadToEndAsync();
        //        }
        //    }
        //}

        public Task WriteAllText(string filePath, string text) {
            return Task.Run(() => {
                File.WriteAllText(filePath, text);
            });
        }
    }
}
