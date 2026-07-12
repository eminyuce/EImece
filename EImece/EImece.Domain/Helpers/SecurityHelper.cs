using System;
using System.IO;

namespace EImece.Domain.Helpers
{
    public static class SecurityHelper
    {
        public static bool IsSafeHttpRedirectUrl(string url, out Uri safeUri)
        {
            safeUri = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out safeUri))
            {
                return false;
            }

            return safeUri.Scheme == Uri.UriSchemeHttp || safeUri.Scheme == Uri.UriSchemeHttps;
        }

        public static bool IsLocalReferrer(Uri referrer, Uri requestUrl)
        {
            if (referrer == null || requestUrl == null)
            {
                return false;
            }

            return referrer.Host.Equals(requestUrl.Host, StringComparison.OrdinalIgnoreCase)
                && referrer.Scheme.Equals(requestUrl.Scheme, StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryGetSafeReferrerRedirect(Uri referrer, Uri requestUrl, out string redirectUrl)
        {
            redirectUrl = null;
            if (!IsLocalReferrer(referrer, requestUrl))
            {
                return false;
            }

            redirectUrl = referrer.ToString();
            return true;
        }

        public static string GetSafeStorageFilePath(string storageRoot, string file)
        {
            if (string.IsNullOrWhiteSpace(storageRoot))
            {
                throw new ArgumentException("Storage root cannot be empty.", nameof(storageRoot));
            }

            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentException("File name cannot be empty.", nameof(file));
            }

            var fileName = Path.GetFileName(file);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Invalid file name.", nameof(file));
            }

            var fullPath = Path.GetFullPath(Path.Combine(storageRoot, fileName));
            var rootPath = Path.GetFullPath(storageRoot);
            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Access to the requested file path is denied.");
            }

            return fullPath;
        }
    }
}
