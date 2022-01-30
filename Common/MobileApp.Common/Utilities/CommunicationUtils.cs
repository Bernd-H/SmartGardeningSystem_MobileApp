using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Exceptions;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.Common.Utilities {
    public static class CommunicationUtils {

        #region Send/Receive Methods

        public static async Task<byte[]> ReceiveAsync(ILogger logger, Stream networkStream, Guid? networkStreamId = null, CancellationToken cancellationToken = default) {
            try {
                return await receiveAsyncWithHeader(networkStream, networkStreamId, cancellationToken);
            }
            catch (IOException ioex) {
                if (ioex.InnerException != null) {
                    if (ioex.InnerException.GetType() == typeof(SocketException)) {
                        var socketE = (SocketException)ioex.InnerException;
                        if (socketE.SocketErrorCode == SocketError.ConnectionReset) {
                            // peer closed the connection
                            throw new ConnectionClosedException(networkStreamId);
                        }
                    }
                }

                throw;
            }
            catch (ObjectDisposedException) {
                throw new ConnectionClosedException(networkStreamId);
            }
        }

        public static byte[] Receive(ILogger logger, Stream networkStream, Guid? networkStreamId = null) {
            try {
                return receiveWithHeader(networkStream, networkStreamId);
            }
            catch (IOException ioex) {
                if (ioex.InnerException != null) {
                    if (ioex.InnerException.GetType() == typeof(SocketException)) {
                        var socketE = (SocketException)ioex.InnerException;
                        if (socketE.SocketErrorCode == SocketError.ConnectionReset) {
                            // peer closed the connection
                            throw new ConnectionClosedException(networkStreamId);
                        }
                    }
                }

                throw;
            }
            catch (ObjectDisposedException) {
                throw new ConnectionClosedException(networkStreamId);
            }
        }

        public static async Task SendAsync(ILogger logger, byte[] msg, Stream networkStream, CancellationToken cancellationToken = default) {
            await sendAsyncWithHeader(msg, networkStream, cancellationToken);
        }

        public static async Task Send(ILogger logger, byte[] msg, Stream networkStream) {
            sendWithHeader(msg, networkStream);
        }


        #region withOUT header

        private static async Task<byte[]> receiveAsyncWithoutHeader(Stream networkStream, Guid? networkStreamId = null, CancellationToken cancellationToken = default) {
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

        private static byte[] receiveWithoutHeader(Stream networkStream, Guid? networkStreamId = null) {
            List<byte> packet = new List<byte>();
            byte[] buffer = new byte[1024];
            int readBytes = 0;
            while (true) {
                readBytes = networkStream.Read(buffer, 0, buffer.Length);

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

        private static async Task sendAsyncWithoutHeader(byte[] msg, Stream networkStream, CancellationToken cancellationToken = default) {
            await networkStream.WriteAsync(msg, 0, msg.Length, cancellationToken);
            await networkStream.FlushAsync();
        }

        private static void sendWithoutHeader(byte[] msg, Stream networkStream) {
            networkStream.Write(msg, 0, msg.Length);
            networkStream.Flush();
        }

        #endregion

        #region with header

        private static async Task<byte[]> receiveAsyncWithHeader(Stream networkStream, Guid? networkStreamId = null, CancellationToken cancellationToken = default) {
            int bytes = -1;
            int packetLength = -1;
            int readBytes = 0;
            List<byte> packet = new List<byte>();

            do {
                byte[] buffer = new byte[2048];
                bytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                // get length
                if (packetLength == -1) {
                    byte[] length = new byte[4];
                    Array.Copy(buffer, 0, length, 0, 4);
                    packetLength = BitConverter.ToInt32(length, 0);
                }

                // add received bytes to the list
                if (bytes != buffer.Length) {
                    readBytes += bytes;
                    byte[] dataToAdd = new byte[bytes];
                    Array.Copy(buffer, 0, dataToAdd, 0, bytes);
                    packet.AddRange(dataToAdd);
                }
                else {
                    readBytes += bytes;
                    packet.AddRange(buffer);
                }

            } while (bytes != 0 && packetLength - readBytes > 0);

            // remove length information and attached bytes
            packet.RemoveRange(packetLength, packet.Count - packetLength);
            packet.RemoveRange(0, 4);

            return packet.ToArray();
        }

        private static byte[] receiveWithHeader(Stream networkStream, Guid? networkStreamId = null) {
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

                // add received bytes to the list
                if (bytes != buffer.Length) {
                    readBytes += bytes;
                    byte[] dataToAdd = new byte[bytes];
                    Array.Copy(buffer, 0, dataToAdd, 0, bytes);
                    packet.AddRange(dataToAdd);
                }
                else {
                    readBytes += bytes;
                    packet.AddRange(buffer);
                }

            } while (bytes != 0 && packetLength - readBytes > 0);

            // remove length information and attached bytes
            packet.RemoveRange(packetLength, packet.Count - packetLength);
            packet.RemoveRange(0, 4);

            return packet.ToArray();
        }

        private static async Task sendAsyncWithHeader(byte[] msg, Stream networkStream, CancellationToken cancellationToken = default) {
            List<byte> packet = new List<byte>();

            // add length of packet - 4B
            packet.AddRange(BitConverter.GetBytes(msg.Length + 4));

            // add content
            packet.AddRange(msg);

            await networkStream.WriteAsync(packet.ToArray(), 0, packet.Count, cancellationToken);
            networkStream.Flush();
        }

        private static void sendWithHeader(byte[] msg, Stream networkStream) {
            List<byte> packet = new List<byte>();

            // add length of packet - 4B
            packet.AddRange(BitConverter.GetBytes(msg.Length + 4));

            // add content
            packet.AddRange(msg);

            networkStream.Write(packet.ToArray(), 0, packet.Count);
            networkStream.Flush();
        }

        #endregion

        #endregion

        public static void ConnectWithTimout(Socket client, IPEndPoint remoteEndPoint, int millisecondsTimeout) {
            var result = client.BeginConnect(remoteEndPoint, null, null);

            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(millisecondsTimeout));

            if (!success) {
                throw new SocketException();
            }

            // we have connected
            client.EndConnect(result);
        }

        public static byte[] SerializeObject<T>(T o) where T : class {
            var json = JsonConvert.SerializeObject(o);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T DeserializeObject<T>(byte[] o) where T : class {
            var json = Encoding.UTF8.GetString(o);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
