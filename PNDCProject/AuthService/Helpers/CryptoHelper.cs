using System.Security.Cryptography;
using System.Text;

namespace AuthService.Helpers
{
    public class CryptoHelper
    {
        // Encrypts a plain text password using AES encryption
        public static (string iv, string cipher) Encrypt(string plainText, string key)
        {
            using var aes = Aes.Create();

            // Set AES key (must be 32 bytes)
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

            // Generate a new IV (Initialization Vector)
            aes.GenerateIV();

            // Create encryptor and convert plain text to bytes
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            // Encrypt the password
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Return both IV and encrypted text as Base64 strings
            return (Convert.ToBase64String(aes.IV), Convert.ToBase64String(cipherBytes));
        }

        // Decrypts an AES encrypted password
        public static string Decrypt(string ivBase64, string cipherBase64, string key)
        {
            using var aes = Aes.Create();

            // Use the same 32-byte key as in encryption
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

            // Convert IV back from Base64
            aes.IV = Convert.FromBase64String(ivBase64);

            // Create decryptor and decrypt the cipher text
            using var decryptor = aes.CreateDecryptor();
            var cipher = Convert.FromBase64String(cipherBase64);
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            // Return the decrypted password as string
            return Encoding.UTF8.GetString(plain);
        }
    }
}
