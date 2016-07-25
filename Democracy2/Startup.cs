using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Democracy2.Startup))]
namespace Democracy2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
