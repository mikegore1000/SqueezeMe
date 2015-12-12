using System.Web.Http;
using Microsoft.Owin;
using Owin;
using SampleApp;
using SqueezeMe;

[assembly: OwinStartup(typeof(Startup))]

namespace SampleApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            app.UseCompression();
            app.UseWebApi(config);
        }
    }
}
