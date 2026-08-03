using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// String encryption helper. Uses AES-256-CBC with random IV and Encrypt-then-MAC (HMAC-SHA256).
    /// Application default passphrase comes from configuration (no hard-coded fallback).
    /// </summary>
    /// <remarks>
    /// Breaking change: payloads from the previous Rijndael/PKCS7 helper (no HMAC, weak PBKDF2)
    /// cannot be decrypted. Re-encrypt any persisted values after deploy.
    /// </remarks>
    public static class StringCipher
    {
        /// <summary>
        /// Application encryption passphrase from env/config. Fails closed if missing.
        /// </summary>
        public static string PassWord
        {
            get
            {
                return EncryptionSecretProvider.GetRawSecret();
            }
        }

        public static string EncryptEscape(string text)
        {
            return Base64UrlEncoder.Encode(Encrypt(text, PassWord));
        }

        public static string DecryptEscape(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                throw new ArgumentException("Cipher text is required.", "cipherText");
            }

            try
            {
                var decoded = Base64UrlEncoder.Decode(cipherText);
                return Decrypt(decoded, PassWord);
            }
            catch (CryptographicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Do not return exception messages (may leak crypto details). Fail closed.
                throw new CryptographicException("Failed to decrypt value.", ex);
            }
        }

        /// <summary>
        /// Encrypts with a caller-supplied passphrase. Random salt and IV per call; HMAC for integrity.
        /// </summary>
        public static string Encrypt(string plainText, string passPhrase)
        {
            return AuthenticatedAesCipher.EncryptWithPassphrase(plainText, passPhrase);
        }

        /// <summary>
        /// Decrypts a value produced by <see cref="Encrypt"/>.
        /// </summary>
        public static string Decrypt(string cipherText, string passPhrase)
        {
            return AuthenticatedAesCipher.DecryptWithPassphrase(cipherText, passPhrase);
        }
    }
}
