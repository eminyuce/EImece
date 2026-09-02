using Swashbuckle.Application;
using System.Web.Http;

namespace EImece
{
    public static class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "EImece REST API")
                    .Description("EImece Open-Source E-Commerce REST and AJAX API surface for mobile and headless frontends.")
                    .Contact(cc => cc.Name("EImece Development Team").Url("https://github.com/eminyuce/EImece"));

                c.DescribeAllEnumsAsStrings();
            })
            .EnableSwaggerUi(c =>
            {
                c.DocumentTitle("EImece API Documentation");
                c.DocExpansion(DocExpansion.List);
            });
        }
    }
}
