using OtpNet;
using QRCoder;
using System.Text;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// TOTP authenticator helpers (Otp.NET + QRCoder) for Google/Microsoft Authenticator.
    /// </summary>
    public static class AuthenticatorHelper
    {
        private const string DefaultIssuer = "Yönetici Paneli";

        public static string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        /// <summary>
        /// Builds otpauth URI shown in authenticator apps as "SiteName:AccountEmail".
        /// </summary>
        public static string GenerateOtpAuthUri(string secret, string accountName, string siteName = null)
        {
            string issuer = NormalizeIssuer(siteName);
            string account = string.IsNullOrWhiteSpace(accountName) ? "admin" : accountName.Trim();
            var uri = new OtpUri(OtpType.Totp, secret, account, issuer);
            return uri.ToString();
        }

        public static string NormalizeIssuer(string siteName)
        {
            if (string.IsNullOrWhiteSpace(siteName))
            {
                return DefaultIssuer;
            }

            // Authenticator labels are clearer without newlines / extreme length.
            string issuer = siteName.Trim();
            if (issuer.Length > 40)
            {
                issuer = issuer.Substring(0, 40).Trim();
            }

            return issuer;
        }

        public static string GenerateQrCodeBase64(string otpAuthUri, int pixelsPerModule = 8)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(otpAuthUri, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
                return System.Convert.ToBase64String(qrCodeBytes);
            }
        }

        public static bool VerifyCode(string secret, string code)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var totp = new Totp(Base32Encoding.ToBytes(secret));
            return totp.VerifyTotp(
                code.Trim(),
                out _,
                new VerificationWindow(previous: 1, future: 1));
        }

        /// <summary>
        /// Formats a Base32 key for manual entry (e.g. "jbsw y3dp ehpk 3pxp").
        /// </summary>
        public static string FormatKey(string unformattedKey)
        {
            if (string.IsNullOrEmpty(unformattedKey))
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }
    }
}
