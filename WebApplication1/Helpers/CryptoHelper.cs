using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Helpers
{
    public static class CryptoHelper
    {
        private static string _encryptionKey;

        public static void Configure(IConfiguration configuration)
        {
            _encryptionKey = configuration["EncryptionSettings:EncryptionKey"];
            if (string.IsNullOrEmpty(_encryptionKey))
                throw new InvalidOperationException("Encryption key is missing in appsettings.json.");
        }

        private static byte[] GetEncryptionKey()
        {
            return Convert.FromBase64String(_encryptionKey);
        }

        private static byte[] GenerateIV()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var iv = new byte[16];
                rng.GetBytes(iv);
                return iv;
            }
        }

        public static (string cipherText, string iv) EncryptString(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.IV = GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                    return (
                        Convert.ToBase64String(encryptedBytes),
                        Convert.ToBase64String(aes.IV)
                    );
                }
            }
        }

        public static string DecryptString(string cipherText, string iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.IV = Convert.FromBase64String(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }
}
