using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// AES-256-CBC with Encrypt-then-MAC (HMAC-SHA256).
    /// Every encryption uses a cryptographically random IV.
    /// <para>
    /// Master-key payload (v1): version(1) || IV(16) || ciphertext || HMAC-SHA256(32)
    /// Passphrase payload (v1): version(1) || salt(16) || IV(16) || ciphertext || HMAC-SHA256(32)
    /// HMAC always covers everything before the MAC bytes.
    /// </para>
    /// AES-GCM is not available on .NET Framework 4.8.1 without extra dependencies;
    /// AES-CBC + HMAC is the recommended authenticated-encryption option here.
    /// Tokens from the previous insecure helpers are intentionally not decryptable.
    /// </summary>
    public static class AuthenticatedAesCipher
    {
        private const byte PayloadVersion = 0x01;
        private const int IvSize = 16;
        private const int SaltSize = 16;
        private const int HmacSize = 32;
        private const int KeySize = 32;
        private const int Pbkdf2Iterations = 100000;

        private static readonly byte[] EncKeyLabel = Encoding.UTF8.GetBytes("EImece-AES-ENC-v1");
        private static readonly byte[] MacKeyLabel = Encoding.UTF8.GetBytes("EImece-HMAC-MAC-v1");

        /// <summary>
        /// Encrypts UTF-8 plaintext with master key material. Returns Base64.
        /// Format: version || IV || ciphertext || HMAC
        /// </summary>
        public static string Encrypt(string plainText, byte[] masterKey)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("plainText");
            }

            ValidateMasterKey(masterKey);

            byte[] encKey = null;
            byte[] macKey = null;
            try
            {
                DeriveSubkeys(masterKey, out encKey, out macKey);
                return Protect(plainText, encKey, macKey, prefixAfterVersion: null);
            }
            finally
            {
                Clear(encKey);
                Clear(macKey);
            }
        }

        /// <summary>
        /// Decrypts a Base64 payload from <see cref="Encrypt"/>.
        /// </summary>
        public static string Decrypt(string cipherText, byte[] masterKey)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                throw new ArgumentException("Cipher text is required.", "cipherText");
            }

            ValidateMasterKey(masterKey);

            byte[] encKey = null;
            byte[] macKey = null;
            try
            {
                DeriveSubkeys(masterKey, out encKey, out macKey);
                return Unprotect(NormalizeBase64(cipherText), encKey, macKey, expectedSaltLength: 0);
            }
            finally
            {
                Clear(encKey);
                Clear(macKey);
            }
        }

        /// <summary>
        /// Password-based encryption with a random per-message salt (PBKDF2-SHA256).
        /// Format: version || salt || IV || ciphertext || HMAC
        /// </summary>
        public static string EncryptWithPassphrase(string plainText, string passPhrase)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("plainText");
            }

            if (string.IsNullOrEmpty(passPhrase))
            {
                throw new ArgumentException("Passphrase is required.", "passPhrase");
            }

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] masterKey = null;
            byte[] encKey = null;
            byte[] macKey = null;
            try
            {
                masterKey = DeriveKeyFromPassphrase(passPhrase, salt);
                DeriveSubkeys(masterKey, out encKey, out macKey);
                return Protect(plainText, encKey, macKey, prefixAfterVersion: salt);
            }
            finally
            {
                Clear(masterKey);
                Clear(encKey);
                Clear(macKey);
                Array.Clear(salt, 0, salt.Length);
            }
        }

        /// <summary>
        /// Decrypts a payload from <see cref="EncryptWithPassphrase"/>.
        /// </summary>
        public static string DecryptWithPassphrase(string cipherText, string passPhrase)
        {
            if (string.IsNullOrEmpty(passPhrase))
            {
                throw new ArgumentException("Passphrase is required.", "passPhrase");
            }

            if (string.IsNullOrWhiteSpace(cipherText))
            {
                throw new ArgumentException("Cipher text is required.", "cipherText");
            }

            var payload = Convert.FromBase64String(NormalizeBase64(cipherText));
            EnsureVersionedPayload(payload, SaltSize);

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(payload, 1, salt, 0, SaltSize);

            byte[] masterKey = null;
            byte[] encKey = null;
            byte[] macKey = null;
            try
            {
                masterKey = DeriveKeyFromPassphrase(passPhrase, salt);
                DeriveSubkeys(masterKey, out encKey, out macKey);
                return Unprotect(NormalizeBase64(cipherText), encKey, macKey, expectedSaltLength: SaltSize);
            }
            finally
            {
                Clear(masterKey);
                Clear(encKey);
                Clear(macKey);
                Array.Clear(salt, 0, salt.Length);
            }
        }

        private static string Protect(string plainText, byte[] encKey, byte[] macKey, byte[] prefixAfterVersion)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var iv = new byte[IvSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            byte[] cipherBytes;
            using (var aes = CreateAes(encKey, iv))
            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(plainBytes, 0, plainBytes.Length);
                    cs.FlushFinalBlock();
                }

                cipherBytes = ms.ToArray();
            }

            Array.Clear(plainBytes, 0, plainBytes.Length);

            var prefixLength = prefixAfterVersion == null ? 0 : prefixAfterVersion.Length;
            var payloadWithoutMac = new byte[1 + prefixLength + IvSize + cipherBytes.Length];
            var offset = 0;
            payloadWithoutMac[offset++] = PayloadVersion;
            if (prefixLength > 0)
            {
                Buffer.BlockCopy(prefixAfterVersion, 0, payloadWithoutMac, offset, prefixLength);
                offset += prefixLength;
            }

            Buffer.BlockCopy(iv, 0, payloadWithoutMac, offset, IvSize);
            offset += IvSize;
            Buffer.BlockCopy(cipherBytes, 0, payloadWithoutMac, offset, cipherBytes.Length);

            byte[] mac;
            using (var hmac = new HMACSHA256(macKey))
            {
                mac = hmac.ComputeHash(payloadWithoutMac);
            }

            var payload = new byte[payloadWithoutMac.Length + HmacSize];
            Buffer.BlockCopy(payloadWithoutMac, 0, payload, 0, payloadWithoutMac.Length);
            Buffer.BlockCopy(mac, 0, payload, payloadWithoutMac.Length, HmacSize);
            return Convert.ToBase64String(payload);
        }

        private static string Unprotect(string cipherTextBase64, byte[] encKey, byte[] macKey, int expectedSaltLength)
        {
            byte[] payload;
            try
            {
                payload = Convert.FromBase64String(cipherTextBase64);
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("Invalid cipher text encoding.", ex);
            }

            EnsureVersionedPayload(payload, expectedSaltLength);

            var macOffset = payload.Length - HmacSize;
            var payloadWithoutMac = new byte[macOffset];
            var providedMac = new byte[HmacSize];
            Buffer.BlockCopy(payload, 0, payloadWithoutMac, 0, macOffset);
            Buffer.BlockCopy(payload, macOffset, providedMac, 0, HmacSize);

            byte[] computedMac;
            using (var hmac = new HMACSHA256(macKey))
            {
                computedMac = hmac.ComputeHash(payloadWithoutMac);
            }

            if (!FixedTimeEquals(providedMac, computedMac))
            {
                throw new CryptographicException("Cipher text authentication failed.");
            }

            var ivOffset = 1 + expectedSaltLength;
            var iv = new byte[IvSize];
            Buffer.BlockCopy(payload, ivOffset, iv, 0, IvSize);

            var cipherOffset = ivOffset + IvSize;
            var cipherBytes = new byte[macOffset - cipherOffset];
            Buffer.BlockCopy(payload, cipherOffset, cipherBytes, 0, cipherBytes.Length);

            using (var aes = CreateAes(encKey, iv))
            using (var decryptor = aes.CreateDecryptor())
            using (var ms = new MemoryStream(cipherBytes))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new MemoryStream())
            {
                cs.CopyTo(reader);
                var plainBytes = reader.ToArray();
                var result = Encoding.UTF8.GetString(plainBytes);
                Array.Clear(plainBytes, 0, plainBytes.Length);
                return result;
            }
        }

        private static void EnsureVersionedPayload(byte[] payload, int expectedSaltLength)
        {
            var minSize = 1 + expectedSaltLength + IvSize + HmacSize + 1;
            if (payload == null || payload.Length < minSize || payload[0] != PayloadVersion)
            {
                throw new CryptographicException(
                    "Invalid or unsupported cipher text. Tokens encrypted with the previous insecure algorithm are no longer valid; re-issue them.");
            }
        }

        private static Aes CreateAes(byte[] encKey, byte[] iv)
        {
            var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = encKey;
            aes.IV = iv;
            return aes;
        }

        private static byte[] DeriveKeyFromPassphrase(string passPhrase, byte[] salt)
        {
            using (var derive = new Rfc2898DeriveBytes(passPhrase, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                return derive.GetBytes(KeySize);
            }
        }

        private static void DeriveSubkeys(byte[] masterKey, out byte[] encKey, out byte[] macKey)
        {
            using (var hmacEnc = new HMACSHA256(masterKey))
            {
                encKey = hmacEnc.ComputeHash(EncKeyLabel);
            }

            using (var hmacMac = new HMACSHA256(masterKey))
            {
                macKey = hmacMac.ComputeHash(MacKeyLabel);
            }
        }

        private static void ValidateMasterKey(byte[] masterKey)
        {
            if (masterKey == null || masterKey.Length < KeySize)
            {
                throw new ArgumentException("Master key must be at least 32 bytes.", "masterKey");
            }
        }

        private static string NormalizeBase64(string cipherText)
        {
            return cipherText.Replace(" ", "+");
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }

        private static void Clear(byte[] buffer)
        {
            if (buffer != null)
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
