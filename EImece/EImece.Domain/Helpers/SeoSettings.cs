using System;
using System.Configuration;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Reads the AllowSearchEngineIndexing appSetting. Indexing is allowed only when the value is exactly "true" (case-insensitive).
    /// </summary>
    public static class SeoSettings
    {
        private const string AllowSearchEngineIndexingKey = "AllowSearchEngineIndexing";

        public static bool AllowIndexing
        {
            get
            {
                return AppConfig.AllowSearchEngineIndexing;
            }
        }
    }
}
