using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackendDev.Helpers
{
    public static class AutoLoginHelper
    {
        public static string GenerateAutoLoginToken(
            string userId,
            string targetUrl,
            string secretKey,
            int expiredMinutes = 30)
        {
            var expires = DateTime.UtcNow.AddMinutes(expiredMinutes);

            // targetUrl di-escape agar aman kalau ada ? atau &
            var safeTargetUrl = Uri.EscapeDataString(targetUrl);

            var payload = $"{userId}|{safeTargetUrl}|{expires:O}";
            var signature = ComputeHmac(payload, secretKey);
            var tokenRaw = $"{payload}|{signature}";

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenRaw))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            return token;
        }

        public static (bool IsValid, string? UserId, string? TargetUrl) ValidateToken(
            string token,
            string secretKey)
        {
            try
            {
                token = token.Replace('-', '+').Replace('_', '/');

                switch (token.Length % 4)
                {
                    case 2: token += "=="; break;
                    case 3: token += "="; break;
                    case 1: throw new FormatException("Invalid base64 token format");
                }

                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split('|');

                if (parts.Length != 4)
                    return (false, null, null);

                var userId = parts[0];
                var safeTargetUrl = parts[1];
                var expiresRaw = parts[2];
                var signature = parts[3];

                var expires = DateTime.ParseExact(
                    expiresRaw,
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

                var payload = $"{userId}|{safeTargetUrl}|{expires:O}";
                var expectedSig = ComputeHmac(payload, secretKey);

                // compare signature lebih aman
                if (!FixedTimeEquals(signature, expectedSig))
                    return (false, null, null);

                if (expires < DateTime.UtcNow)
                    return (false, null, null);

                var targetUrl = Uri.UnescapeDataString(safeTargetUrl);

                return (true, userId, targetUrl);
            }
            catch
            {
                return (false, null, null);
            }
        }

        private static string ComputeHmac(string input, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);

            return leftBytes.Length == rightBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        public static (bool IsValid, string? UserId, string? TargetUrl, string? Error) ValidateTokenDebug(
            string token,
            string secretKey)
        {
            try
            {
                token = token.Replace('-', '+').Replace('_', '/');

                switch (token.Length % 4)
                {
                    case 2: token += "=="; break;
                    case 3: token += "="; break;
                    case 1: return (false, null, null, "Invalid base64 token format");
                }

                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split('|');

                if (parts.Length != 4)
                    return (false, null, null, $"Jumlah bagian token tidak valid. Parts={parts.Length}");

                var userId = parts[0];
                var safeTargetUrl = parts[1];
                var expiresRaw = parts[2];
                var signature = parts[3];

                var expires = DateTime.ParseExact(
                    expiresRaw,
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

                var payload = $"{userId}|{safeTargetUrl}|{expires:O}";
                var expectedSig = ComputeHmac(payload, secretKey);

                if (!FixedTimeEquals(signature, expectedSig))
                    return (false, null, null, "Signature mismatch. SecretKey generate dan validate kemungkinan berbeda.");

                if (expires < DateTime.UtcNow)
                    return (false, null, null, $"Expired. Exp={expires:O}, Now={DateTime.UtcNow:O}");

                var targetUrl = Uri.UnescapeDataString(safeTargetUrl);

                return (true, userId, targetUrl, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
            }
        }
    }
}
