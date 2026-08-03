using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class AuthenticatedAesCipherTests
    {
        private string _testPassphrase;

        [TestInitialize]
        public void TestInitialize()
        {
            // Generate an ephemeral test secret at runtime so no password-like
            // value is committed to source control (GitGuardian / secret scanners).
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            _testPassphrase = Convert.ToBase64String(bytes);
            EncryptionSecretProvider.ClearCache();
            Environment.SetEnvironmentVariable(
                EncryptionSecretProvider.EnvironmentVariableName,
                _testPassphrase);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            EncryptionSecretProvider.ClearCache();
            Environment.SetEnvironmentVariable(
                EncryptionSecretProvider.EnvironmentVariableName,
                null);
            _testPassphrase = null;
        }

        [TestMethod]
        public void EncryptDecryptQueryString_RoundTrips()
        {
            var plain = "order-guid-abc-123";
            var cipher = EncryptDecryptQueryString.Encrypt(plain);
            var decrypted = EncryptDecryptQueryString.Decrypt(cipher);
            Assert.AreEqual(plain, decrypted);
        }

        [TestMethod]
        public void EncryptDecryptQueryString_ProducesDifferentCiphertextEachTime()
        {
            var plain = "same-plaintext";
            var c1 = EncryptDecryptQueryString.Encrypt(plain);
            var c2 = EncryptDecryptQueryString.Encrypt(plain);
            Assert.AreNotEqual(c1, c2);
            Assert.AreEqual(plain, EncryptDecryptQueryString.Decrypt(c1));
            Assert.AreEqual(plain, EncryptDecryptQueryString.Decrypt(c2));
        }

        [TestMethod]
        public void EncryptDecryptQueryString_RejectsTamperedCiphertext()
        {
            var cipher = EncryptDecryptQueryString.Encrypt("user-id-42");
            var bytes = Convert.FromBase64String(cipher);
            bytes[bytes.Length - 5] ^= 0xFF;
            var tampered = Convert.ToBase64String(bytes);

            try
            {
                EncryptDecryptQueryString.Decrypt(tampered);
                Assert.Fail("Expected CryptographicException");
            }
            catch (CryptographicException)
            {
                // expected
            }
        }

        [TestMethod]
        public void EncryptDecryptQueryString_RejectsLegacyInsecurePayload()
        {
            // Legacy payloads had no version/HMAC prefix; must fail closed.
            var legacyLooking = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            try
            {
                EncryptDecryptQueryString.Decrypt(legacyLooking);
                Assert.Fail("Expected CryptographicException");
            }
            catch (CryptographicException)
            {
                // expected
            }
        }

        [TestMethod]
        public void StringCipher_RoundTripsWithPassphrase()
        {
            var plain = "sensitive-value";
            var cipher = StringCipher.Encrypt(plain, _testPassphrase);
            var decrypted = StringCipher.Decrypt(cipher, _testPassphrase);
            Assert.AreEqual(plain, decrypted);
        }

        [TestMethod]
        public void StringCipher_EncryptEscape_RoundTrips()
        {
            var plain = "callback-token";
            var escaped = StringCipher.EncryptEscape(plain);
            var decrypted = StringCipher.DecryptEscape(escaped);
            Assert.AreEqual(plain, decrypted);
        }

        [TestMethod]
        public void EncryptionSecretProvider_FailsClosedWhenMissing()
        {
            EncryptionSecretProvider.ClearCache();
            Environment.SetEnvironmentVariable(
                EncryptionSecretProvider.EnvironmentVariableName,
                "short");
            EncryptionSecretProvider.ClearCache();

            try
            {
                EncryptionSecretProvider.GetMasterKey();
                Assert.Fail("Expected InvalidOperationException for weak key");
            }
            catch (InvalidOperationException)
            {
                // expected
            }
        }

        [TestMethod]
        public void AuthenticatedAesCipher_MasterKeyRoundTrip()
        {
            var key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            var plain = "hello-aes";
            var cipher = AuthenticatedAesCipher.Encrypt(plain, key);
            Assert.AreEqual(plain, AuthenticatedAesCipher.Decrypt(cipher, key));
        }
    }
}
