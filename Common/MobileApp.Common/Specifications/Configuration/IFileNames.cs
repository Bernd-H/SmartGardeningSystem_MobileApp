namespace MobileApp.Common.Specifications.Configuration {
    public interface IFileNames {
        string SettingsFileName { get; set; }

        string SecureStorageKey_SettingsFileEncryptionKey { get; set; }
    }
}
