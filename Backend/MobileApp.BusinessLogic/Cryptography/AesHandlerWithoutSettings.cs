using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.Entities.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.Managers;
using NLog;

namespace MobileApp.BusinessLogic.Cryptography {

    /// <inheritdoc/>
    public class AesHandlerWithoutSettings : IAesHandlerWithoutSettings {

        private ILogger Logger;

        public AesHandlerWithoutSettings(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<AesHandlerWithoutSettings>();
        }

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data, byte[] key, byte[] iv) {
            try {
                Logger.Trace($"[Decrypt]Decrypting byte array with length={data.Length}.");
                return DecryptByteArray(data, key, iv);
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[Decrypt2]Failed to decrypt data.");
                throw;
            }
        }

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data, byte[] key, byte[] iv) {
            try {
                Logger.Trace($"[Encrypt]Encrypting string with length={data.Length}.");
                return EncryptByteArray(data, key, iv);
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[Encrypt2]Error while encrypting data.");
                throw;
            }
        }

        /// <inheritdoc/>
        public (byte[], byte[]) CreateAesKeyIv() {
            var config = ConfigurationStore.GetConfig();
            int keySize = Convert.ToInt32(config.AesKeyLength_Bytes);

            using (var myRijndael = new RijndaelManaged()) {
                myRijndael.KeySize = keySize * 8;
                myRijndael.GenerateKey();
                myRijndael.GenerateIV();

                return (myRijndael.Key, myRijndael.IV);
            }
        }

        static byte[] EncryptByteArray(byte[] data, byte[] key, byte[] iv) {
            using (RijndaelManaged rijAlg = new RijndaelManaged()) {
                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                rijAlg.Padding = PaddingMode.Zeros;

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        int chunkSize = 1024;

                        for (int i = 0; i < data.Length; i += chunkSize) {
                            byte[] buffer = new byte[chunkSize];

                            // copy data into the buffer
                            if (i + chunkSize >= data.Length) {
                                Array.Copy(data, i, buffer, 0, data.Length - i);
                            }
                            else {
                                Array.Copy(data, i, buffer, 0, chunkSize);
                            }

                            csEncrypt.Write(buffer, 0, buffer.Length);
                        }

                        csEncrypt.Flush();
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray().Prepend(BitConverter.GetBytes(data.Length));
                    }
                }
            }
        }

        static byte[] DecryptByteArray(byte[] encryptedBytesWithLength, byte[] key, byte[] iv) {
            // get length of data
            (byte[] lengthBytes, byte[] encryptedData) = encryptedBytesWithLength.Shift(4);
            var length = BitConverter.ToInt32(lengthBytes, 0);

            using (RijndaelManaged rijAlg = new RijndaelManaged()) {
                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                rijAlg.Padding = PaddingMode.Zeros;

                // Create the streams used for decryption. 
                using (MemoryStream mstream = new MemoryStream(encryptedData)) {
                    using (CryptoStream cryptoStream = new CryptoStream(mstream, decryptor, CryptoStreamMode.Read)) {
                        //byte[] decryptedData = new byte[length];
                        //cryptoStream.Read(decryptedData, 0, length);
                        //cryptoStream.Flush();
                        //return decryptedData;

                        int bytes = -1;
                        int readBytes = 0;
                        List<byte> decryptedData = new List<byte>();

                        do {
                            byte[] buffer = new byte[1024];

                            bytes = cryptoStream.Read(buffer, 0, buffer.Length);

                            readBytes += bytes;
                            decryptedData.AddRange(buffer);

                        } while (bytes != 0 && length - readBytes > 0);

                        // remove length information and attached bytes
                        decryptedData.RemoveRange(length, decryptedData.Count - length);

                        return decryptedData.ToArray();
                    }
                }
            }
        }
    }
}
