using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;

[assembly: OwinStartupAttribute(typeof(EImece.Startup))]

namespace EImece
{
    public partial class Startup
    {
        public static IDataProtectionProvider DataProtectionProvider { get; set; }

        public void Configuration(IAppBuilder app)
        {
            DataProtectionProvider = app.GetDataProtectionProvider();
            ConfigureAuth(app);
        }
    }
}