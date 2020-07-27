using System.Web.Mvc;

namespace EImece.Areas.Customers
{
    public class CustomersAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Customers";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "Customers_default",
               "Customers/{controller}/{action}/{id}",
                     new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                     new[] { "EImece.Areas.Customers.Controllers" }
           );
        }
    }
}