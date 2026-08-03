using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Loads the application encryption secret from a secure location.
    /// Precedence: environment variable <c>EIMECE_ENCRYPTION_KEY</c>, then
    /// <c>appSettings["encrypt-password"]</c>. Fails closed if missing or too weak.
    /// Never logs the secret value.
    /// </summary>
    public static class EncryptionSecretProvider
    {
        public const string AppSettingKey = "encrypt-password";
        public const string EnvironmentVariableName = "EIMECE_ENCRYPTION_KEY";

        /// <summary>Minimum length of the configured secret string (characters).</summary>
        public const int MinimumSecretLength = 32;

        private static readonly object SyncRoot = new object();
        private static byte[] _cachedMasterKey;
        private static string _cachedSecretSourceFingerprint;

        /// <summary>
        /// Returns 32 bytes of master key material derived from the configured secret.
        /// Throws <see cref="InvalidOperationException"/> if the secret is not configured.
        /// </summary>
        public static byte[] GetMasterKey()
        {
            var secret = GetRawSecret();
            EnsureSecretStrength(secret);

            // Cache by content fingerprint without retaining the plaintext secret string longer than needed.
            var fingerprint = ComputeFingerprint(secret);
            lock (SyncRoot)
            {
                if (_cachedMasterKey != null
                    && string.Equals(_cachedSecretSourceFingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return Clone(_cachedMasterKey);
                }

                var masterKey = DeriveMasterKey(secret);
                _cachedMasterKey = Clone(masterKey);
                _cachedSecretSourceFingerprint = fingerprint;
                return masterKey;
            }
        }

        /// <summary>
        /// Returns the raw secret string. Prefer the environment variable over Web.config.
        /// </summary>
        public static string GetRawSecret()
        {
            var fromEnv = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv.Trim();
            }

            var fromConfig = ConfigurationManager.AppSettings[AppSettingKey];
            if (!string.IsNullOrWhiteSpace(fromConfig))
            {
                return fromConfig.Trim();
            }

            throw new InvalidOperationException(
                "Encryption key is not configured. Set the "
                + EnvironmentVariableName
                + " environment variable, or add appSettings key '"
                + AppSettingKey
                + "' in Web.config / App.config. "
                + "Generate a strong key (e.g. openssl rand -base64 32). "
                + "The application will not fall back to a hard-coded key.");
        }

        /// <summary>
        /// Clears any cached key material (for tests or key rotation within a process).
        /// </summary>
        public static void ClearCache()
        {
            lock (SyncRoot)
            {
                if (_cachedMasterKey != null)
                {
                    Array.Clear(_cachedMasterKey, 0, _cachedMasterKey.Length);
                    _cachedMasterKey = null;
                }

                _cachedSecretSourceFingerprint = null;
            }
        }

        private static void EnsureSecretStrength(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "Encryption key is empty. Configure a strong secret of at least "
                    + MinimumSecretLength
                    + " characters.");
            }

            // Accept Base64 key material of at least 32 decoded bytes, or a long passphrase.
            byte[] decoded;
            if (TryDecodeBase64(secret, out decoded) && decoded.Length >= 32)
            {
                Array.Clear(decoded, 0, decoded.Length);
                return;
            }

            if (decoded != null)
            {
                Array.Clear(decoded, 0, decoded.Length);
            }

            if (secret.Length < MinimumSecretLength)
            {
                throw new InvalidOperationException(
                    "Encryption key is too weak. Use a Base64-encoded 32-byte key "
                    + "(openssl rand -base64 32) or a secret of at least "
                    + MinimumSecretLength
                    + " characters.");
            }
        }

        private static byte[] DeriveMasterKey(string secret)
        {
            byte[] decoded;
            if (TryDecodeBase64(secret, out decoded) && decoded.Length >= 32)
            {
                // High-entropy key material already; fold to exactly 32 bytes via SHA-256.
                using (var sha = SHA256.Create())
                {
                    var hash = sha.ComputeHash(decoded);
                    Array.Clear(decoded, 0, decoded.Length);
                    return hash;
                }
            }

            if (decoded != null)
            {
                Array.Clear(decoded, 0, decoded.Length);
            }

            // Passphrase path: stretch with PBKDF2-SHA256.
            // Salt is a public domain-separation constant (not a secret); strength comes from the passphrase.
            var salt = Encoding.UTF8.GetBytes("EImece.Domain.Encryption.v1");
            using (var derive = new Rfc2898DeriveBytes(secret, salt, 100000, HashAlgorithmName.SHA256))
            {
                return derive.GetBytes(32);
            }
        }

        private static bool TryDecodeBase64(string value, out byte[] bytes)
        {
            bytes = null;
            try
            {
                bytes = System.Convert.FromBase64String(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string ComputeFingerprint(string secret)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
                // Only enough to detect config changes; not reversible to the secret in practice for logging.
                return System.Convert.ToBase64String(hash, 0, 8);
            }
        }

        private static byte[] Clone(byte[] source)
        {
            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }
    }
}
