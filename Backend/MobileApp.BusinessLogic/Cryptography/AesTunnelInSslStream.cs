using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Utilities;
using NLog;

namespace MobileApp.BusinessLogic.Cryptography {

    /// <inheritdoc/>
    public class AesTunnelInSslStream : IAesTunnelInSslStream {

        private static SemaphoreSlim LOCKER = new SemaphoreSlim(1);

        private SslStream _sslSteram;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        private ILogger Logger;

        public AesTunnelInSslStream(ILoggerService loggerService, IAesEncrypterDecrypter aesEncrypterDecrypter) {
            Logger = loggerService.GetLogger<AesTunnelInSslStream>();
            AesEncrypterDecrypter = aesEncrypterDecrypter;
        }

        public void Init(SslStream sslStream) {
            _sslSteram = sslStream;
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReceiveData(CancellationToken cancellationToken = default) {
            Logger.Trace($"[ReceiveData]Waiting to receive encrypted data.");
            byte[] encryped = await CommunicationUtils.ReceiveAsync(null, _sslSteram, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) {
                return null;
            }

            return AesEncrypterDecrypter.Decrypt(encryped);
        }

        /// <inheritdoc/>
        public async Task<byte[]> SendAndReceiveData(byte[] data, CancellationToken cancellationToken = default) {
            try {
                await LOCKER.WaitAsync();

                Logger.Info($"[SendAndReceiveData]Sending {data.Length} bytes through the relay tunnel.");
                await SendData(data, cancellationToken);
                var received = await ReceiveData(cancellationToken);
                Logger.Info($"[SendAndReceiveData]Received {received.Length} bytes from the relay tunnel.");

                return received;
            }
            finally {
                LOCKER.Release();
            }
        }

        /// <inheritdoc/>
        public async Task SendData(byte[] data, CancellationToken cancellationToken = default) {
            Logger.Trace($"[SendData]Sending {data.Length} bytes encrypted.");

            byte[] encrypedMessage = AesEncrypterDecrypter.Encrypt(data);

            //CommunicationUtils.Send(null, encrypedMessage, _sslSteram);
            await CommunicationUtils.SendAsync(null, encrypedMessage, _sslSteram, cancellationToken);
        }
    }
}
