using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using System.Globalization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

[assembly: OwinStartupAttribute(typeof(EImece.Startup))]

namespace EImece
{
    public partial class Startup
    {
        public static IDataProtectionProvider DataProtectionProvider { get; set; }

        public void Configuration(IAppBuilder app)
        {
            // Data Protection Provider'ı al
            DataProtectionProvider = app.GetDataProtectionProvider();

            // Authentication yapılandırması
            ConfigureAuth(app);

            // Türkçe yerelleştirme için kültür ayarlarını yap
            var cultureInfo = new CultureInfo(Domain.Constants.TR);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
           
        }
    }
}