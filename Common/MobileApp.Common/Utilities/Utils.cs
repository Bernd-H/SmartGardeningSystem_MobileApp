using System;
using System.Text;
using Newtonsoft.Json;

namespace MobileApp.Common.Utilities {
    public static class Utils {

        public static T ConvertValue<T, T1>(T1 value) where T1 : struct {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        #region Hex conversions

        public static string ConvertByteToHex(byte b) {
            return BitConverter.ToString(new byte[] { b });
        }

        public static string ConvertByteArrayToHex(byte[] b) {
            return BitConverter.ToString(b);
        }

        public static byte ConvertHexToByte(string hex) {
            var b = ConvertHexToByteArray(hex);
            if (b.Length != 1) {
                throw new ArgumentException();
            }

            return b[0];
        }

        public static byte[] ConvertHexToByteArray(string hex) {
            // from https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array/321404
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i) {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex) {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        #endregion

        #region TimeSpan Extensions

        public static string ToReadableString(this TimeSpan span) {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        #endregion
    }
}
