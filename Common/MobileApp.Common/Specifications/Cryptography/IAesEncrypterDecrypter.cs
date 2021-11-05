using System;

namespace MobileApp.Common.Specifications.Cryptography {
    public interface IAesEncrypterDecrypter {

        /// <summary>
        /// Encrypts data with a stored aes key.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Encrypt(string data);

        /// <summary>
        /// Encryptes data with given key and iv.
        /// </summary>
        /// <exception cref="Exception">Thrown if something went wrong.</exception>
        byte[] Encrypt(byte[] data, byte[] key, byte[] iv);

        /// <summary>
        /// Decrypts data with a stored aes key.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>string.Empty when data made no sense.</returns>
        string Decrypt(byte[] data);


        /// <summary>
        /// Decrypts data with given key and iv.
        /// </summary>
        /// <exception cref="Exception">Thrown if something went wrong.</exception>
        byte[] Decrypt(byte[] data, byte[] key, byte[] iv);
    }
}
