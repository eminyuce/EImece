﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            catch (Exception e)
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