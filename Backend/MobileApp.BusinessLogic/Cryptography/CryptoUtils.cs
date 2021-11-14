using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.BusinessLogic.Cryptography {
    public static class CryptoUtils {

        public static (byte[] left, byte[] right) Shift(this byte[] bytes, int size) {
            var left = new byte[size];
            var right = new byte[bytes.Length - size];

            Array.Copy(bytes, 0, left, 0, left.Length);
            Array.Copy(bytes, left.Length, right, 0, right.Length);

            return (left, right);
        }

        public static byte[] Prepend(this byte[] bytes, byte[] bytesToPrepend) {
            var tmp = new byte[bytes.Length + bytesToPrepend.Length];
            bytesToPrepend.CopyTo(tmp, 0);
            bytes.CopyTo(tmp, bytesToPrepend.Length);
            return tmp;
        }
    }
}
