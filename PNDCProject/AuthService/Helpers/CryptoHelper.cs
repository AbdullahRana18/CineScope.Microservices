using System.Security.Cryptography;
using System.Text;
namespace AuthService.Helpers
{
    public class CryptoHelper
    {
        // Encrypt password using AES
        public static (string iv, string cipher) Encrypt(string PlainText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(PlainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return (Convert.ToBase64String(aes.IV), Convert.ToBase64String(cipherBytes));
        }



        // Decrypt AES password

        public static string Decrypt(string ivBase64, string cipherBase64, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = Convert.FromBase64String(ivBase64);


            using var decryptor = aes.CreateDecryptor();
            var cipher = Convert.FromBase64String(cipherBase64);
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }
    }
}
