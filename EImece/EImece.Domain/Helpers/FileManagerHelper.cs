using System;
using System.Text;
using System.Web;

namespace EImece.Domain.Helpers
{
    public class FileManagerHelper
    {
        public static String GetServiceWorkerScript()
        {
            String filePath = "~/Scripts/serviceWorker/mainToReplace.js";
            string[] lines = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath(filePath));
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (!String.IsNullOrEmpty(line) && !line.StartsWith("//"))
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }
    }
}