using System;
using System.IO;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using NLog;
using Renci.SshNet;

namespace MobileApp.DataAccess {
    public class SftpClient : Common.Specifications.DataAccess.ISftpClient {

        private ILogger Logger;

        public SftpClient(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<SftpClient>();
        }

        public Task<bool> UploadFile(string host, string username, string password, Stream input, string filepath, Stream privateKey) {
            return Task.Run(() => {
                Logger.Info($"[UploadFile]Uploading {filepath} to sftp: {host}");

                try {
                    //var connectionInfo = new ConnectionInfo(host, username, new PasswordAuthenticationMethod(username, password),
                    //new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(privateKey)));

                    var connectionInfo = new ConnectionInfo(host, username, new PasswordAuthenticationMethod(username, password));

                    using (var client = new Renci.SshNet.SftpClient(connectionInfo)) {
                        client.Connect();

                        client.UploadFile(input, filepath);

                        return true;
                    }
                }
                catch (Exception ex) {
                    Logger.Error(ex, "[UploadFile]Uploading file failed.");
                }

                return false;
            });
        }
    }
}
