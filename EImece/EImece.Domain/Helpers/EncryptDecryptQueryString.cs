using System;
using System.Security.Cryptography;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Encrypts/decrypts values used in query strings (e.g. payment callback o/u parameters).
    /// Uses AES-256-CBC with a random IV and Encrypt-then-MAC (HMAC-SHA256).
    /// The encryption key must be configured; there is no hard-coded fallback.
    /// </summary>
    /// <remarks>
    /// Breaking change: ciphertext produced by the previous implementation (fixed salt/IV,
    /// hard-coded key fallback) cannot be decrypted. In-flight payment callback URLs become
    /// invalid after deploy; users must restart checkout. This is intentional for security.
    /// </remarks>
    public static class EncryptDecryptQueryString
    {
        public static string Encrypt(string clearText)
        {
            if (clearText == null)
            {
                throw new ArgumentNullException("clearText");
            }

            var masterKey = EncryptionSecretProvider.GetMasterKey();
            try
            {
                return AuthenticatedAesCipher.Encrypt(clearText, masterKey);
            }
            finally
            {
                Array.Clear(masterKey, 0, masterKey.Length);
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                throw new ArgumentException("Cipher text is required.", "cipherText");
            }

            var masterKey = EncryptionSecretProvider.GetMasterKey();
            try
            {
                return AuthenticatedAesCipher.Decrypt(cipherText, masterKey);
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("Failed to decrypt query string value.", ex);
            }
            finally
            {
                Array.Clear(masterKey, 0, masterKey.Length);
            }
        }
    }
}
