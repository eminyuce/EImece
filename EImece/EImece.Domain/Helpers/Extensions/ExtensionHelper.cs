using System;
using System.Web;

namespace EImece.Domain.Helpers.Extensions
{
    public static class ExtensionHelper
    {
        public static Byte[] ToByteArray(this HttpPostedFileBase value)
        {
            if (value == null)
                return null;

            var array = new Byte[value.ContentLength];
            value.InputStream.Position = 0;
            value.InputStream.Read(array, 0, value.ContentLength);
            return array;
        }
    }
}