using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Utilities;
using NLog;

namespace MobileApp.BusinessLogic.Cryptography {
    public class AesTunnelInSslStream : IAesTunnelInSslStream {

        private static SemaphoreSlim LOCKER = new SemaphoreSlim(1);

        private SslStream _sslSteram;

        private ISettingsManager SettingsManager;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        private ILogger Logger;

        public AesTunnelInSslStream(ILoggerService loggerService, IAesEncrypterDecrypter aesEncrypterDecrypter, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<AesTunnelInSslStream>();
            AesEncrypterDecrypter = aesEncrypterDecrypter;
            SettingsManager = settingsManager;
        }

        public void Init(SslStream sslStream) {
            _sslSteram = sslStream;
        }

        public async Task<byte[]> ReceiveData() {
            Logger.Trace($"[ReceiveData]Waiting to receive encrypted data.");
            byte[] encryped = CommunicationUtils.Receive(null, _sslSteram);

            var settings = await SettingsManager.GetApplicationSettings();

            return AesEncrypterDecrypter.Decrypt(encryped, settings.AesKey, settings.AesIV);
        }

        public async Task<byte[]> SendAndReceiveData(byte[] msg) {
            await LOCKER.WaitAsync();

            await SendData(msg);
            var received = await ReceiveData();

            LOCKER.Release();
            return received;
        }

        public async Task SendData(byte[] msg) {
            Logger.Trace($"[SendData]Sending {msg.Length} bytes encrypted.");
            var settings = await SettingsManager.GetApplicationSettings();

            byte[] encrypedMessage = AesEncrypterDecrypter.Encrypt(msg, settings.AesKey, settings.AesIV);

            CommunicationUtils.Send(null, encrypedMessage, _sslSteram);
        }
    }
}
