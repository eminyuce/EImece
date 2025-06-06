﻿using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    /// <summary>
    /// Attribute that can be added to controller methods to force content
    /// to be GZip encoded if the client supports it
    /// </summary>
    public class CompressContentAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Override to compress the content that is generated by
        /// an action method.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            GZipEncodePage();
        }

        /// <summary>
        /// Determines if GZip is supported
        /// </summary>
        /// <returns></returns>
        public static bool IsGZipSupported()
        {
            string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(AcceptEncoding) &&
                    (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate")))
                return true;
            return false;
        }

        /// <summary>
        /// Sets up the current page or handler to use GZip through a Response.Filter
        /// IMPORTANT:
        /// You have to call this method before any output is generated!
        /// </summary>
        public static void GZipEncodePage()
        {
            HttpResponse Response = HttpContext.Current.Response;

            if (IsGZipSupported())
            {
                string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

                if (AcceptEncoding.Contains("gzip"))
                {
                    Response.Filter = new System.IO.Compression.GZipStream(Response.Filter,
                                                System.IO.Compression.CompressionMode.Compress);
                    Response.Headers.Remove("Content-Encoding");
                    Response.AppendHeader("Content-Encoding", "gzip");
                }
                else
                {
                    Response.Filter = new System.IO.Compression.DeflateStream(Response.Filter,
                                                System.IO.Compression.CompressionMode.Compress);
                    Response.Headers.Remove("Content-Encoding");
                    Response.AppendHeader("Content-Encoding", "deflate");
                }
            }

            // Allow proxy servers to cache encoded and unencoded versions separately
            Response.AppendHeader("Vary", "Content-Encoding");
        }
    }
}