namespace MobileApp.Common.Specifications.Cryptography {

    /// <summary>
    /// Class to encrypt and decrypt byte arrays with RijndaelManaged.
    /// </summary>
    public interface IAesEncrypterDecrypter {

        /// <summary>
        /// Encrypts a string with the Aes key, stored in the application settings.
        /// </summary>
        /// <param name="data">Data to encrypt.</param>
        /// <returns>Encrypted data.</returns>
        byte[] Encrypt(string data);

        /// <summary>
        /// Encrypts a byte array with the Aes key, stored in the application settings.
        /// </summary>
        /// <param name="data">Data to encrypt.</param>
        /// <returns>Encrypted data.</returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypts a byte array with the Aes key, stored in the application settings.
        /// </summary>
        /// <param name="data">Data to decrypt.</param>
        /// <returns>Decrypted data.</returns>
        byte[] Decrypt(byte[] data);
    }
}
