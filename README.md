# SqueezeMe

SqueezeMe provides an OWIN middleware that enables both GZIP and Deflate response compression.

Sample usage

```C#
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
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            app.UseCompression();
            app.UseWebApi(config);
        }
    }
}
```