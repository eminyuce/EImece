using System.Collections.Generic;
using System.Xml.Linq;

namespace EImece.Domain.Helpers.SiteMap
{
    public interface ISitemapGenerator
    {
        XDocument GenerateSiteMap(IEnumerable<ISitemapItem> items);

        XDocument GenerateNewsSiteMap(IEnumerable<ISitemapItem> items);
    }
}