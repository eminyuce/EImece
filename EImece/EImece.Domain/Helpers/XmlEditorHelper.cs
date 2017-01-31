using RazorEngine;
using RazorEngine.Templating; // For extension methods.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EImece.Domain.Helpers
{
    public class XmlEditorHelper
    {
        public String GenerateRss()
        {
            string url = HttpContext.Current.Server.MapPath("~/App_Data/rssTemplate.txt");
            var template = File.ReadAllText(url);
            String result = Engine.Razor.RunCompile(template, "templateKeyRss");
            return result;
        }
        public String GenerateXmlEditor()
        {
            string url = HttpContext.Current.Server.MapPath("~/App_Data/xmlEditorRazor.txt");
            var template = File.ReadAllText(url);
            var config = GetConfiguration();


            String result = Engine.Razor.RunCompile(template, 
                "templateKey1", 
                null, 
                new { Children = config.Item1,
                    Configurations = config.Item2,
                    GroupAttributes = GetValue("group", "names") });
            return result;
        }

        private Tuple<String, String> GetConfiguration()
        {
            string url = HttpContext.Current.Server.MapPath("~/App_Data/xmlEditorConfiguration.txt");
            //  var configurations = File.ReadAllText(url);

            var configurationLines = File.ReadAllLines(url).ToList();
            String componentListLine = configurationLines.FirstOrDefault(r => r.StartsWith("//componentList:"));
            var componentList = componentListLine.Replace("//componentList:", "").Split(",".ToCharArray()).Select(r => r.Trim());

            configurationLines.Remove(componentListLine);
            var configurations = String.Join(" ", configurationLines);

            var configurationComponent = String.Format("[{0}]", string.Join(",", componentList.Select(x => string.Format("'{0}'", x.Trim()))));
            foreach (var component in componentList)
            {
                configurations = configurations.Replace(component + "names", GetValue(component, "names"));
                configurations = configurations.Replace(component + "units", GetValue(component, "units"));
                configurations = configurations.Replace(component + "values", GetValue(component, "values"));
            }



            return new Tuple<String, String>(configurationComponent, configurations);
        }
        private String GetValue(string tag, string atribute)
        {
            var r = String.Format("[{0}]", string.Join(",", GetList(tag, atribute).Select(x => string.Format("'{0}'", x.Trim()))));
            r = GeneralHelper.GetStringTitleCase(r);
            return r;
        }
        private List<string> GetList(string tag, string atribute)
        {
            if (atribute.Equals("names", StringComparison.InvariantCultureIgnoreCase))
            {

                if (tag.Equals("group", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new List<string> { "Size", "Power", "Other" };
                }

                return new List<string> { "width", "length", "hight" };
            }

            if (atribute.Equals("units", StringComparison.InvariantCultureIgnoreCase))
            {
                return new List<string> { "inch", "volt", "pound" };
            }
            if (atribute.Equals("values", StringComparison.InvariantCultureIgnoreCase))
            {
                return new List<string> { "Countries", "Colors", "State" };
            }

            return new List<string>
            {

            };
        }


    }
}
