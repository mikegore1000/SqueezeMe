using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using SqueezeMe;

[assembly: OwinStartup(typeof(SampleApp.App_Start.Startup))]

namespace SampleApp.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            app.Use(typeof(CompressionMiddleware));
            app.UseWebApi(config);
        }
    }
}
