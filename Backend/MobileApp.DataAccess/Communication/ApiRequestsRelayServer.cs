using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.DataObjects;
using MobileApp.Common.Utilities;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.DataAccess.Communication {

    /// <inheritdoc/>
    public class ApiRequestsRelayServer : IApiRequestsRelayServer {

        private static ManualResetEvent _tcpClientConnected = new ManualResetEvent(false);

        private CancellationToken _cancellationToken;

        private SemaphoreSlim _locker = new SemaphoreSlim(1);


        private IEncryptedTunnel _relayTunnel;

        private ILogger Logger;

        public ApiRequestsRelayServer(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<ApiRequestsRelayServer>();
        }

        /// <inheritdoc/>
        public Task<bool> Start(IEncryptedTunnel relayTunnel, CancellationToken cancellationToken) {
            bool success = false;
            _relayTunnel = relayTunnel;
            _cancellationToken = cancellationToken;

            try {
                // start listening on the same port as the API listens on the basestation
                //int port = ConfigurationStore.GetConfig().ConnectionSettings.API_Port;
                int port = 5000;
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Server.Blocking = true;
                listener.Start();

                cancellationToken.Register(() => listener.Stop());

                Task.Run(() => {
                    while (true) {
                        _tcpClientConnected.Reset();

                        listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), listener);

                        _tcpClientConnected.WaitOne();
                    }
                }, cancellationToken);

                success = true;
            }
            catch (Exception ex) {
                Logger.Fatal(ex, $"[Start]An error orrued while starting API-relay-server.");
            }

            return Task.FromResult(success);
        }

        private async void AcceptTcpClientCallback(IAsyncResult ar) {
            TcpClient client = null;
            NetworkStream networkStream = null;
            bool resetEventSet = false;

            try {
                TcpListener listener = (TcpListener)ar.AsyncState;

                client = listener.EndAcceptTcpClient(ar);
                client.Client.Blocking = true;

                // Signal the calling thread to continue.
                _tcpClientConnected.Set();
                resetEventSet = true;

                networkStream = client.GetStream();

                // get all data
                byte[] data = await Receive(networkStream);

                // pack data to an object
                IWanPackage wanPackage = new WanPackage() {
                    Package = data,
                    PackageType = PackageType.Relay,
                    ServiceDetails = new ServiceDetails() {
                        //Port = ConfigurationStore.GetConfig().ConnectionSettings.API_Port,
                        Port = 5000,
                        Type = ServiceType.API
                    }
                };
                var wanPackageJson = JsonConvert.SerializeObject(wanPackage);

                // tunnel data threw tunnel and wait for answer
                byte[] answer = await _relayTunnel.SendAndReceiveData(Encoding.UTF8.GetBytes(wanPackageJson));

                // deserialize wan package
                var answerWanPackage = JsonConvert.DeserializeObject<WanPackage>(Encoding.UTF8.GetString(answer));

                // send answer back to request maker
                await Send(answerWanPackage.Package, networkStream);
            }
            catch (Exception ex) {
                Logger.Fatal(ex, $"[AcceptTcpClientCallback]An error occured in api relay server.");
            }
            finally {
                //Console.WriteLine("Closing connection.");
                networkStream?.Close();
                client?.Close();

                if (!resetEventSet) {
                    _tcpClientConnected.Set();
                }
            }
        }

        private async Task<byte[]> Receive(NetworkStream networkStream) {
            return await CommunicationUtils.ReceiveAsyncWithoutHeader(Logger, networkStream);
        }

        private async Task Send(byte[] msg, NetworkStream networkStream) {
            await CommunicationUtils.SendAsyncWithoutHeader(Logger, msg, networkStream);
        }

        #region old

        private static byte[] GetRequestOrAnswer(Stream inputStream) {
            //Read Request Line
            string request = Readline(inputStream);

            string[] tokens = request.Split(' ');
            //if (tokens.Length != 3) {
            //    throw new Exception("invalid http request line");
            //}
            //string method = tokens[0].ToUpper();
            //string url = tokens[1];
            //string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            string allheaders = request + "\r\n";
            while ((line = Readline(inputStream)) != null) {
                if (line.Equals("")) {
                    break;
                }

                allheaders += (line + "\r\n");

                int separator = line.IndexOf(':');
                if (separator == -1) {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' ')) {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            string content = null;
            if (headers.ContainsKey("Content-Length")) {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];

                while (bytesLeft > 0) {
                    byte[] buffer = new byte[bytesLeft > 1024 ? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }

            //return (allheaders, content);
            return MergeRequestHeaderAndContent(allheaders, content);
        }

        private static void WriteRequest(Stream stream, string requestAndHeader, string content) {
            //Write(stream, requestAndHeader + "\r\n");
            //Write(stream, content);
            var bytes = MergeRequestHeaderAndContent(requestAndHeader, content);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteRequest(Stream stream, byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
        }

        private static byte[] MergeRequestHeaderAndContent(string requestAndHeader, string content) {
            return Encoding.UTF8.GetBytes(requestAndHeader + "\r\n" + content);
        }

        private static string Readline(Stream stream) {
            int next_char;
            string data = "";
            while (true) {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        private static void Write(Stream stream, string text) {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        #endregion
    }
}
