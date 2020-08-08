using System;
using System.Security.Cryptography;

namespace EImece.Domain.Helpers
{
    public class HashHelpers
    {
        public static string GetSha256Hash(byte[] bytes)
        {
            string ret = "";

            try
            {
                HashAlgorithm sha256 = new SHA256Managed();
                byte[] bHash = sha256.ComputeHash(bytes);
                ret = BitConverter.ToString(bHash).Replace("-", "");
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
                // throw;
            }

            return ret;
        }

        public static bool IsCorrectHash(string hash)
        {
            return hash.Length == 64;
        }
    }
}