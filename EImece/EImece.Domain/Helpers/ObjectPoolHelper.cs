using Microsoft.Extensions.ObjectPool;
using System;
using System.Text;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Thread-safe pooling helper for high-throughput operations using Microsoft.Extensions.ObjectPool.
    /// </summary>
    public static class ObjectPoolHelper
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool =
            new DefaultObjectPoolProvider().CreateStringBuilderPool();

        public static StringBuilder RentStringBuilder()
        {
            return StringBuilderPool.Get();
        }

        public static void ReturnStringBuilder(StringBuilder sb)
        {
            if (sb != null)
            {
                StringBuilderPool.Return(sb);
            }
        }

        public static string BuildString(Action<StringBuilder> builderAction)
        {
            if (builderAction == null)
            {
                return string.Empty;
            }

            var sb = StringBuilderPool.Get();
            try
            {
                builderAction(sb);
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }
    }
}
