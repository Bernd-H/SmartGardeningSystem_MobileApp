using MobileApp.Common.Specifications.Configuration;

namespace MobileApp.Common.Models.Entities.Configuration {
    public class FileNames : IFileNames {
        public string SettingsFileName { get; set; }

        public string SecureStorageKey_SettingsFileEncryptionKey { get; set; }
    }
}
