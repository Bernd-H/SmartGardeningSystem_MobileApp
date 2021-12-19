using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Exceptions;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.DataObjects;
using MobileApp.Common.Utilities;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.DataAccess.Communication {
    public class CommandsRelayServer : ICommandsRelayServer {

        private static ManualResetEvent _tcpClientConnected = new ManualResetEvent(false);

        private IEncryptedTunnel _relayTunnel;

        private CancellationToken _cancellationToken;


        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        private ILogger Logger;
        
        public CommandsRelayServer(ILoggerService loggerService, IAesEncrypterDecrypter aesEncrypterDecrpyter) {
            Logger = loggerService.GetLogger<CommandsRelayServer>();
            AesEncrypterDecrypter = aesEncrypterDecrpyter;
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
                Guid networkStreamId = new Guid();

                // get all data
                Guid serviceSessionId = Guid.Empty;
                try {
                    while (!_cancellationToken.IsCancellationRequested) {
                        byte[] data = await Receive(networkStream, networkStreamId);

                        IServicePackage dataPackage = new ServicePackage() {
                            Data = data,
                            SessionId = serviceSessionId
                        };

                        // pack data to an object
                        IWanPackage wanPackage = new WanPackage() {
                            Package = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataPackage)),
                            PackageType = PackageType.Relay,
                            ServiceDetails = new ServiceDetails() {
                                Port = ConfigurationStore.GetConfig().ConnectionSettings.CommandsListener_Port,
                                Type = ServiceType.AesTcp,
                                HoldConnectionOpen = true
                            }
                        };
                        var wanPackageJson = JsonConvert.SerializeObject(wanPackage);

                        // tunnel data threw tunnel and wait for answer
                        byte[] answer = await _relayTunnel.SendAndReceiveData(Encoding.UTF8.GetBytes(wanPackageJson));

                        // deserialize wan package
                        var answerWanPackage = JsonConvert.DeserializeObject<WanPackage>(Encoding.UTF8.GetString(answer));

                        // get session id
                        var answerPackage = JsonConvert.DeserializeObject<ServicePackage>(Encoding.UTF8.GetString(answerWanPackage.Package));
                        if (serviceSessionId == Guid.Empty) {
                            serviceSessionId = answerPackage.SessionId;
                        }

                        // send answer back to request maker
                        await Send(answerPackage.Data, networkStream);
                    }
                }catch (ConnectionClosedException cce) {
                    if (cce.NetworkStreamId == networkStreamId) {
                        if (serviceSessionId != Guid.Empty) {
                            // relaytunnel connection still open, the local connection to this relay server got closed
                            // send close connection request for the relayserver at the project GardeningSystem
                            IWanPackage wanPackage = new WanPackage() {
                                Package = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ServicePackage() {
                                    Data = new byte[0],
                                    SessionId = serviceSessionId
                                })),
                                PackageType = PackageType.Relay,
                                ServiceDetails = new ServiceDetails() {
                                    Port = ConfigurationStore.GetConfig().ConnectionSettings.CommandsListener_Port,
                                    Type = ServiceType.AesTcp,
                                    HoldConnectionOpen = false
                                }
                            };
                            var wanPackageJson = JsonConvert.SerializeObject(wanPackage);

                            Logger.Info($"[AcceptTcpClientCallback]Closing connection to command service.");
                            _ = await _relayTunnel.SendAndReceiveData(Encoding.UTF8.GetBytes(wanPackageJson));
                        }
                        else {
                            // connection will eventually timeout
                        }
                    }
                    else {
                        // (ex thrown in SendAndReceiveData())
                        // Tunnel connection got closed
                        Logger.Warn($"[AcceptTcpClientCallback]Connection to basestation got closed. Abording command relay.");
                    }
                }
                finally {
                    client?.Close();
                }
            }
            catch (Exception ex) {
                Logger.Fatal(ex, $"[AcceptTcpClientCallback]An error occured in api relay server.");
            }
        }

        //private async Task<byte[]> Receive(NetworkStream networkStream, Guid networkStreamId) {
        //    try {
        //        List<byte> packet = new List<byte>();
        //        byte[] buffer = new byte[1024];
        //        int readBytes = 0;
        //        while (true) {
        //            readBytes = await networkStream.ReadAsync(buffer, 0, buffer.Length, _cancellationToken);

        //            if (readBytes == 0) {
        //                throw new ConnectionClosedException(networkStreamId);
        //            }
        //            if (readBytes < buffer.Length) {
        //                var tmp = new List<byte>(buffer);
        //                packet.AddRange(tmp.GetRange(0, readBytes));
        //                break;
        //            }
        //            else {
        //                packet.AddRange(buffer);
        //            }
        //        }

        //        return packet.ToArray();
        //    }
        //    catch (ObjectDisposedException) {
        //        throw new ConnectionClosedException(networkStreamId);
        //    }
        //}

        private async Task<byte[]> Receive(NetworkStream networkStream, Guid networkStreamId) {
            return await CommunicationUtils.ReceiveAsync(Logger, networkStream, networkStreamId);
        }

        //private async Task Send(byte[] msg, NetworkStream networkStream) {
        //    await networkStream.WriteAsync(msg, 0, msg.Length, _cancellationToken);
        //    networkStream.Flush();
        //}

        public async Task Send(byte[] msg, NetworkStream networkStream) {
            Logger.Info($"[SendData] Sending data with length {msg.Length}.");

            await CommunicationUtils.SendAsync(Logger, msg, networkStream);
        }
    }
}
