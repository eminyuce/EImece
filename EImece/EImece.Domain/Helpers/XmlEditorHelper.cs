using EImece.Domain.Services.IServices;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public class XmlEditorHelper
    {
        private readonly IListService ListService;

        public XmlEditorHelper(IListService listService)
        {
            ListService = listService ?? throw new ArgumentNullException(nameof(listService));
        }

        private static string ResolveAppDataPath(string fileName)
        {
            var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            if (!string.IsNullOrWhiteSpace(dataDir))
            {
                return Path.Combine(dataDir, fileName);
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", fileName);
        }

        public String GenerateXmlEditor(int id = 0)
        {
            string url = ResolveAppDataPath("xmlEditorRazor.txt");
            var template = File.ReadAllText(url);
            var config = GetConfiguration();

            String result = Engine.Razor.RunCompile(template,
                "templateKey_" + id,
                null,
                new
                {
                    Children = config.Item1,
                    Configurations = config.Item2,
                    GroupAttributes = GetValue("group", "names")
                });
            return result;
        }

        private Tuple<String, String> GetConfiguration()
        {
            string url = ResolveAppDataPath("xmlEditorConfiguration.txt");

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

        private List<string> GetListItems(String name)
        {
            var list = ListService.GetListItems();
            var listItem = list.FirstOrDefault(r => r.Name.Equals(name));
            if (listItem != null)
            {
                var i = listItem.ListItems.OrderBy(r => r.Position).Select(r => r.Value).ToList();
                return i;
            }
            else
            {
                return new List<string>();
            }
        }

        private List<string> GetList(string tag, string atribute)
        {
            if (atribute.Equals("names", StringComparison.InvariantCultureIgnoreCase))
            {
                if (tag.Equals("group", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetListItems("ComponentDisplayNames");
                }

                return GetListItems("ComponentNames");
            }

            if (atribute.Equals("units", StringComparison.InvariantCultureIgnoreCase))
            {
                return GetListItems("ComponentUnit");
            }

            if (atribute.Equals("values", StringComparison.InvariantCultureIgnoreCase))
            {
                var list = ListService.GetListItems();
                return list.Where(r => r.IsValues).OrderBy(r => r.Position).Select(r => r.Name).ToList();
            }

            return new List<string>();
        }
    }
}