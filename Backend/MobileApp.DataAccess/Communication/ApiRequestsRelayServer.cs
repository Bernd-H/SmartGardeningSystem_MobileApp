using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess.Communication;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class ApiRequestsRelayServer : IApiRequestsRelayServer {

        private static ManualResetEvent _tcpClientConnected = new ManualResetEvent(false);

        private IEncryptedTunnel _relayTunnel;


        private ILogger Logger;

        public ApiRequestsRelayServer(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<ApiRequestsRelayServer>();
        }

        public Task<bool> Start(IEncryptedTunnel relayTunnel, CancellationToken cancellationToken) {
            bool success = false;
            _relayTunnel = relayTunnel;

            try {
                TcpListener listener = new TcpListener(IPAddress.Any, 5035);
                listener.Start();

                Console.WriteLine($"Listening on {listener.Server.LocalEndPoint.ToString()}...");

                // Set the event to nonsignaled state.
                _tcpClientConnected.Reset();

                // Start to listen for connections from a client.
                Console.WriteLine("Waiting for a connection...");

                // Accept the connection.
                // BeginAcceptSocket() creates the accepted socket.
                listener.BeginAcceptTcpClient(
                    new AsyncCallback(DoAcceptTcpClientCallback),
                    listener);

                // Wait until a connection is made and processed before
                // continuing.
                _tcpClientConnected.WaitOne();
            }
            catch (Exception ex) {
                Logger.Fatal(ex, $"[Start]An error orrued while starting API-relay-server.");
            }

            return success;
        }

        public static void DoAcceptTcpClientCallback(IAsyncResult ar) {
            TcpListener listener = (TcpListener)ar.AsyncState;

            TcpClient client = listener.EndAcceptTcpClient(ar);

            Console.WriteLine($"Client with endpoint={client.Client.RemoteEndPoint.ToString()} accepted.");

            // Signal the calling thread to continue.
            _tcpClientConnected.Set();

            var networkStream = client.GetStream();


            // send to external server
            using (var sendToAPIClient = new TcpClient()) {
                sendToAPIClient.Connect(IPAddress.Parse("127.0.0.1"), 5000);
                var sendToAPIClient_NS = sendToAPIClient.GetStream();

                string reqAndHeader, content;
                Console.WriteLine("Get request");
                (reqAndHeader, content) = GetRequestOrAnswer(networkStream);
                Console.WriteLine(reqAndHeader);
                Console.WriteLine(content);
                Console.WriteLine("forward request");
                WriteRequest(sendToAPIClient_NS, reqAndHeader, content);
                sendToAPIClient_NS.Flush();


                // get answer and send it back to request maker
                Console.WriteLine("Receiving answer");
                (reqAndHeader, content) = GetRequestOrAnswer(sendToAPIClient_NS);
                Console.WriteLine(reqAndHeader);
                Console.WriteLine(content);
                Console.WriteLine("Sending answer");
                WriteRequest(networkStream, reqAndHeader, content);
                networkStream.Flush();

                Console.WriteLine("Finished.");

                client.Close();
            }
        }


        private static (string, string) GetRequestOrAnswer(Stream inputStream) {
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

            return (allheaders, content);
        }

        private static void WriteRequest(Stream stream, string requestAndHeader, string content) {
            Write(stream, requestAndHeader + "\r\n");
            Write(stream, content);
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
    }
}
