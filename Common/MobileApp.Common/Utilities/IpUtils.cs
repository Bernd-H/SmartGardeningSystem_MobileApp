using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MobileApp.Common.Utilities {
    public static class IpUtils {
        public static IPAddress GetHostAddress(string domain, int millisecondsTimeout) {
            var task = Task<IPAddress>.Factory.StartNew(() => {
                try {
                    return Dns.GetHostAddresses(domain).FirstOrDefault();
                }
                catch (Exception) {
                    return null;
                }
            });

            bool success = task.Wait(millisecondsTimeout);
            if (success) {
                return task.Result;
            }
            else {
                return null;
            }
        }

        public static bool IsHostAvailable(string ipAddress) {
            bool pingable = false;
            Ping pinger = null;

            try {
                pinger = new Ping();
                PingReply reply = pinger.Send(ipAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException) {
                // Discard PingExceptions and return false;
            }
            finally {
                if (pinger != null) {
                    pinger.Dispose();
                }
            }

            return pingable;
        }

        /// <summary>
        /// Handles IPv4 and IPv6 notation.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns>Null when parsing failed.</returns>
        public static IPEndPoint IPEndPoint_TryParse(string endPoint) {
            try {
                string[] ep = endPoint.Split(':');
                if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
                IPAddress ip;
                if (ep.Length > 2) {
                    if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip)) {
                        throw new FormatException("Invalid ip-adress");
                    }
                }
                else {
                    if (!IPAddress.TryParse(ep[0], out ip)) {
                        throw new FormatException("Invalid ip-adress");
                    }
                }
                int port;
                if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port)) {
                    throw new FormatException("Invalid port");
                }
                return new IPEndPoint(ip, port);
            }
            catch (FormatException) {
                return null;
            }
        }
    }
}
