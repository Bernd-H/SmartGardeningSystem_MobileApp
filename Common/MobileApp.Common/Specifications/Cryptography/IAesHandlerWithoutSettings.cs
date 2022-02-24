namespace MobileApp.Common.Specifications.Cryptography {

    public interface IAesHandlerWithoutSettings {

        byte[] Encrypt(byte[] data, byte[] key, byte[] iv);

        byte[] Decrypt(byte[] data, byte[] key, byte[] iv);

        (byte[], byte[]) CreateAesKeyIv();
    }
}
