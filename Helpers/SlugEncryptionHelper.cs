using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackendDev.Helpers
{
    public static class SlugEncryptionHelper
    {
        public static string EncryptGuid(Guid id, string secretKey)
        {
            var plainText = id.ToString("D");
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var aes = Aes.Create();
            aes.Key = BuildKey(secretKey);
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // gabungkan IV + CipherText
            var combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            return ToBase64Url(combined);
        }

        public static Guid? DecryptGuid(string slug, string secretKey)
        {
            try
            {
                var combined = FromBase64Url(slug);

                using var aes = Aes.Create();
                aes.Key = BuildKey(secretKey);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var ivLength = aes.BlockSize / 8;
                if (combined.Length <= ivLength) return null;

                var iv = new byte[ivLength];
                var cipherBytes = new byte[combined.Length - ivLength];

                Buffer.BlockCopy(combined, 0, iv, 0, ivLength);
                Buffer.BlockCopy(combined, ivLength, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                var plainText = Encoding.UTF8.GetString(plainBytes);

                return Guid.TryParse(plainText, out var guid) ? guid : null;
            }
            catch
            {
                return null;
            }
        }

        private static byte[] BuildKey(string secretKey)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
        }

        private static string ToBase64Url(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static byte[] FromBase64Url(string input)
        {
            input = input.Replace('-', '+').Replace('_', '/');

            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
            }

            return Convert.FromBase64String(input);
        }
    }
}