using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PowerBiSample.Startup))]
namespace PowerBiSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
