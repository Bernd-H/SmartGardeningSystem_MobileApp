using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Exceptions;
using NLog;

namespace MobileApp.Common.Utilities {
    public static class CommunicationUtils {

        #region Async-Methods

        public static async Task<byte[]> ReceiveAsync(ILogger logger, Stream networkStream, CancellationToken cancellationToken = default) {
            try {
                List<byte> packet = new List<byte>();
                byte[] buffer = new byte[1024];
                int readBytes = 0;
                while (true) {
                    readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (readBytes == 0) {
                        //throw new ConnectionClosedException(networkStreamId);
                    }
                    if (readBytes < buffer.Length) {
                        var tmp = new List<byte>(buffer);
                        packet.AddRange(tmp.GetRange(0, readBytes));
                        break;
                    }
                    else {
                        packet.AddRange(buffer);
                    }
                }

                return packet.ToArray();
            }
            catch (ObjectDisposedException) {
                //throw new ConnectionClosedException(networkStreamId);
            }

            return new byte[0];
        }


        public static async Task<byte[]> ReceiveAsync(ILogger logger, Stream networkStream, Guid networkStreamId, CancellationToken cancellationToken = default) {
            try {
                List<byte> packet = new List<byte>();
                byte[] buffer = new byte[1024];
                int readBytes = 0;
                while (true) {
                    readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (readBytes == 0) {
                        throw new ConnectionClosedException(networkStreamId);
                    }
                    if (readBytes < buffer.Length) {
                        var tmp = new List<byte>(buffer);
                        packet.AddRange(tmp.GetRange(0, readBytes));
                        break;
                    }
                    else {
                        packet.AddRange(buffer);
                    }
                }

                return packet.ToArray();
            }
            catch (ObjectDisposedException) {
                throw new ConnectionClosedException(networkStreamId);
            }
        }

        public static async Task SendAsync(ILogger logger, byte[] msg, Stream networkStream, CancellationToken cancellationToken = default) {
            await networkStream.WriteAsync(msg, 0, msg.Length, cancellationToken);
            await networkStream.FlushAsync();
        }

        #endregion

        #region Sync-Methods

        public static byte[] Receive(ILogger logger, Stream networkStream) {
            try {
                List<byte> packet = new List<byte>();
                byte[] buffer = new byte[1024];
                int readBytes = 0;
                while (true) {
                    readBytes = networkStream.Read(buffer, 0, buffer.Length);

                    if (readBytes == 0) {
                        //throw new ConnectionClosedException(networkStreamId);
                        throw new Exception();
                    }
                    if (readBytes < buffer.Length) {
                        var tmp = new List<byte>(buffer);
                        packet.AddRange(tmp.GetRange(0, readBytes));
                        break;
                    }
                    else {
                        packet.AddRange(buffer);
                    }
                }

                return packet.ToArray();
            }
            catch (ObjectDisposedException) {
                //throw new ConnectionClosedException(networkStreamId);
            }

            return new byte[0];
        }

        public static void Send(ILogger logger, byte[] msg, Stream networkStream) {
            networkStream.Write(msg, 0, msg.Length);
            networkStream.Flush();
        }

        #endregion

        #region Obsolete methods

        [Obsolete]
        public static byte[] ReceiveDataWithHeader(Stream networkStream) {
            int bytes = -1;
            int packetLength = -1;
            int readBytes = 0;
            List<byte> packet = new List<byte>();

            do {
                byte[] buffer = new byte[2048];
                bytes = networkStream.Read(buffer, 0, buffer.Length);

                // get length
                if (packetLength == -1) {
                    byte[] length = new byte[4];
                    Array.Copy(buffer, 0, length, 0, 4);
                    packetLength = BitConverter.ToInt32(length, 0);
                }

                readBytes += bytes;
                packet.AddRange(buffer);

            } while (bytes != 0 && packetLength - readBytes > 0);

            // remove length information and attached bytes
            packet.RemoveRange(packetLength, packet.Count - packetLength);
            packet.RemoveRange(0, 4);

            return packet.ToArray();
        }

        [Obsolete]
        public static void SendDataWithHeader(byte[] msg, Stream networkStream) {
            List<byte> packet = new List<byte>();

            // add length of packet - 4B
            packet.AddRange(BitConverter.GetBytes(msg.Length + 4));

            // add content
            packet.AddRange(msg);

            networkStream.Write(packet.ToArray(), 0, packet.Count);
            networkStream.Flush();
        }

        #endregion
    }
}
