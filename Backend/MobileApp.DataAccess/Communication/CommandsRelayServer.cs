using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class CommandsRelayServer : ICommandsRelayServer {

        private static ManualResetEvent _tcpClientConnected = new ManualResetEvent(false);

        private IEncryptedTunnel _relayTunnel;

        private CancellationToken _cancellationToken;


        private ILogger Logger;
        
        public CommandsRelayServer(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<CommandsRelayServer>();
        }

        public Task<bool> Start(IEncryptedTunnel relayTunnel, CancellationToken cancellationToken) {
            bool success = false;
            _relayTunnel = relayTunnel;
            _cancellationToken = cancellationToken;

            try {
                // start listening on the same port as the CommandManager on the basestation
                int port = ConfigurationStore.GetConfig().ConnectionSettings.CommandsListener_Port;
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
            try {
                TcpListener listener = (TcpListener)ar.AsyncState;

                TcpClient client = listener.EndAcceptTcpClient(ar);
                client.Client.Blocking = true;

                // Signal the calling thread to continue.
                _tcpClientConnected.Set();

                var networkStream = client.GetStream();

                // get all data
                while (!_cancellationToken.IsCancellationRequested) {
                    byte[] data = await Receive(networkStream);

                    // pack data to an object
                    IWanPackage wanPackage = new WanPackage() {
                        Package = data,
                        PackageType = PackageType.Relay,
                        ServiceDetails = new ServiceDetails() {
                            Port = ConfigurationStore.GetConfig().ConnectionSettings.CommandsListener_Port,
                            Type = ServiceType.API
                        }
                    };
                    var wanPackageJson = JsonConvert.SerializeObject(wanPackage);

                    // tunnel data threw tunnel and wait for answer
                    //await _relayTunnel.SendData(Encoding.UTF8.GetBytes(wanPackageJson));
                    //byte[] answer = await _relayTunnel.ReceiveData();
                    byte[] answer = await _relayTunnel.SendAndReceiveData(Encoding.UTF8.GetBytes(wanPackageJson));

                    // deserialize wan package
                    var answerWanPackage = JsonConvert.DeserializeObject<WanPackage>(Encoding.UTF8.GetString(answer));

                    // send answer back to request maker
                    await Send(answerWanPackage.Package, networkStream);
                }
            }
            catch (Exception ex) {
                Logger.Fatal(ex, $"[AcceptTcpClientCallback]An error occured in api relay server.");
            }
        }

        private async Task<byte[]> Receive(NetworkStream networkStream) {
            List<byte> packet = new List<byte>();
            byte[] buffer = new byte[1024];
            int readBytes = 0;
            while (true) {
                readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, _cancellationToken);

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

        private async Task Send(byte[] msg, NetworkStream networkStream) {
            await networkStream.WriteAsync(msg, 0, msg.Length, _cancellationToken);
            networkStream.Flush();
        }
    }
}
