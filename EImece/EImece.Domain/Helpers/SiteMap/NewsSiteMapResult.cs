using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace EImece.Domain.Helpers.SiteMap
{
    public class NewsSiteMapResult : ActionResult
    {
        private readonly IEnumerable<ISitemapItem> items;
        private readonly ISitemapGenerator generator;

        public NewsSiteMapResult(IEnumerable<ISitemapItem> items)
            : this(items, new SitemapGenerator())
        {
        }

        public NewsSiteMapResult(IEnumerable<ISitemapItem> items, ISitemapGenerator generator)
        {
            // Ensure.Argument.NotNull(items, "items");
            // Ensure.Argument.NotNull(generator, "generator");

            this.items = items;
            this.generator = generator;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;

            response.ContentType = "text/xml";
            response.ContentEncoding = Encoding.UTF8;

            using (var writer = new XmlTextWriter(response.Output))
            {
                writer.Formatting = Formatting.Indented;
                var sitemap = generator.GenerateNewsSiteMap(items);

                sitemap.WriteTo(writer);
            }
        }
    }
}