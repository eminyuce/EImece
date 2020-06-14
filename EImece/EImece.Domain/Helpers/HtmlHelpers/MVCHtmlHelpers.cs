using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.HtmlHelpers
{
    public static class MVCHtmlHelpers
    {
        public static IHtmlString EmbedCss(this HtmlHelper htmlHelper, string path)
        {
            // take a path that starts with "~" and map it to the filesystem.
            var cssFilePath = HttpContext.Current.Server.MapPath(path);
            // load the contents of that file
            try
            {
                if (!File.Exists(cssFilePath))
                {
                    return htmlHelper.Raw("!-- No file exists '" + path + "' -->");
                }
                var cssText = File.ReadAllText(cssFilePath);
                return htmlHelper.Raw("<style>\n" + cssText + "\n</style>");
            }
            catch
            {
                return htmlHelper.Raw("!-- Can't read the file '" + path + "' -->");
            }
        }

        public static IHtmlString EmbedCustomCss(this HtmlHelper htmlHelper, string basePath = "~/Content/css/")
        {
            var controller = htmlHelper.ViewContext.RouteData.GetRequiredString("controller").ToLowerInvariant();
            var action = htmlHelper.ViewContext.RouteData.GetRequiredString("action").ToLowerInvariant();
            var suffix = (htmlHelper.ViewContext.HttpContext.IsDebuggingEnabled) ? ".css" : ".min.css";

            var cssFiles = new List<string>() {
                basePath + controller + suffix,
                basePath + controller + "-" + action + suffix
            };

            // take a path that starts with "~" and map it to the filesystem.

            // load the contents of that file
            var allCssContent = new StringBuilder();
            var allCssFiles = new StringBuilder();
            try
            {
                foreach (var cssFile in cssFiles)
                {
                    allCssFiles.AppendLine("<!-- " + cssFile + " -->");
                }
                foreach (var cssFile in cssFiles)
                {
                    var cssFilePath = HttpContext.Current.Server.MapPath(cssFile);
                    if (!File.Exists(cssFilePath)) continue;
                    allCssContent.AppendLine("<style>");
                    allCssContent.AppendLine(File.ReadAllText(cssFilePath));
                    allCssContent.AppendLine("</style>");
                }
            }
            catch (Exception ex)
            {
                allCssFiles.AppendLine("<!-- " + ex.Message + " -->");
            }
            return htmlHelper.Raw(allCssFiles.ToString() + allCssContent.ToString());
        }
    }
}