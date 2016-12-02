using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EImece.Startup))]
namespace EImece
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
