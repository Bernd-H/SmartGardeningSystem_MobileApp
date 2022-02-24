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
    public class AesEncrypterDecrypter : IAesEncrypterDecrypter {

        private ISettingsManager SettingsManager;

        private ILogger Logger;

        public AesEncrypterDecrypter(ILoggerService loggerService, ISettingsManager settingsManager) {
            SettingsManager = settingsManager;
            Logger = loggerService.GetLogger<AesEncrypterDecrypter>();
        }

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] data) {
            try {
                Logger.Trace($"[Decrypt]Decrypting byte array with length={data.Length}.");
                var settings = SettingsManager.GetApplicationSettings().Result;
                return DecryptByteArray(data, settings.AesKey, settings.AesIV);
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[Decrypt]Failed to decrypt data.");
                throw;
            }
        }

        /// <inheritdoc/>
        public byte[] Encrypt(string data) {
            try {
                Logger.Trace($"[Encrypt]Encrypting string with length={data.Length}.");
                var settings = SettingsManager.GetApplicationSettings().Result;
                return EncryptByteArray(Encoding.UTF8.GetBytes(data), settings.AesKey, settings.AesIV);
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[Encrypt]Error while encrypting data.");
                throw;
            }
        }

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data) {
            try {
                Logger.Trace($"[Encrypt]Encrypting string with length={data.Length}.");
                var settings = SettingsManager.GetApplicationSettings().Result;
                return EncryptByteArray(data, settings.AesKey, settings.AesIV);
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[Encrypt]Error while encrypting data.");
                throw;
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

        #region old
        //static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV) {
        //    // Check arguments. 
        //    if (plainText == null || plainText.Length <= 0)
        //        throw new ArgumentNullException("plainText");
        //    if (Key == null || Key.Length <= 0)
        //        throw new ArgumentNullException("Key");
        //    if (IV == null || IV.Length <= 0)
        //        throw new ArgumentNullException("IV");
        //    byte[] encrypted;
        //    // Create an RijndaelManaged object 
        //    // with the specified key and IV. 
        //    using (RijndaelManaged rijAlg = new RijndaelManaged()) {
        //        rijAlg.Key = Key;
        //        rijAlg.IV = IV;

        //        // Create a decryptor to perform the stream transform.
        //        ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

        //        // Create the streams used for encryption. 
        //        using (MemoryStream msEncrypt = new MemoryStream()) {
        //            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
        //                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {

        //                    //Write all data to the stream.
        //                    swEncrypt.Write(plainText);
        //                }
        //                encrypted = msEncrypt.ToArray();
        //            }
        //        }
        //    }


        //    // Return the encrypted bytes from the memory stream. 
        //    return encrypted;
        //}

        //static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV) {
        //    // Check arguments. 
        //    if (cipherText == null || cipherText.Length <= 0)
        //        throw new ArgumentNullException("cipherText");
        //    if (Key == null || Key.Length <= 0)
        //        throw new ArgumentNullException("Key");
        //    if (IV == null || IV.Length <= 0)
        //        throw new ArgumentNullException("IV");

        //    // Declare the string used to hold 
        //    // the decrypted text. 
        //    string plaintext = null;

        //    // Create an RijndaelManaged object 
        //    // with the specified key and IV. 
        //    using (RijndaelManaged rijAlg = new RijndaelManaged()) {
        //        rijAlg.Key = Key;
        //        rijAlg.IV = IV;

        //        // Create a decrytor to perform the stream transform.
        //        ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

        //        // Create the streams used for decryption. 
        //        using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
        //            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
        //                using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {

        //                    // Read the decrypted bytes from the decrypting stream 
        //                    // and place them in a string.
        //                    plaintext = srDecrypt.ReadToEnd();
        //                }
        //            }
        //        }

        //    }

        //    return plaintext;

        //}

        #endregion
    }
}
