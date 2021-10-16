using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Cryptography {
    public interface IAesEncrypterDecrypter {

        /// <summary>
        /// Encrypts data with a stored aes key.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Encrypt(string data);

        /// <summary>
        /// Decrypts data with a stored aes key.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>string.Empty when data made no sense.</returns>
        string Decrypt(byte[] data);
    }
}
