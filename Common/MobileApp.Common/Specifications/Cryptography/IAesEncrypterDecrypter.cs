using System;

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
        /// Encryptes data with given key and iv.
        /// </summary>
        /// <param name="data">Data to encrypt.</param>
        /// <param name="iv">Aes initialization vector.</param>
        /// <param name="key">Aes key.</param>
        /// <returns>Encrypted data.</returns>
        /// <exception cref="Exception">Thrown if something went wrong.</exception>
        byte[] Encrypt(byte[] data, byte[] key, byte[] iv);

        /// <summary>
        /// Decrypts data with a stored aes key.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>string.Empty when data made no sense.</returns>
        //string Decrypt(byte[] data);

        /// <summary>
        /// Decrypts data with given key and iv.
        /// </summary>
        /// <param name="data">Data to decrypt.</param>
        /// <param name="iv">Aes initialization vector.</param>
        /// <param name="key">Aes key.</param>
        /// <returns>Decrypted data.</returns>
        /// <exception cref="Exception">Thrown if something went wrong.</exception>
        byte[] Decrypt(byte[] data, byte[] key, byte[] iv);
    }
}
