# SqueezeMe

SqueezeMe provides an OWIN middleware that enables both GZIP and Deflate response compression.

## Sample usage

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
			
            // Adds the SqueezeMe middleware to the pipeline, no config required by default.
            app.UseCompression(); 

            // However, you can supply a list of MIME types to exclude from compression instead.
            app.UseCompression(excludedMimeTypes: new [] { "application/xml" }); 

            app.UseWebApi(config);
        }
    }
}
```