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

            // var excludes = null;            // null = use internal list
            // var excludes = new string[0];   // empty array = compress everything, exclude nothing
            var excludes = new[] { "image/tiff", "video/mpeg2" };     // or override with exclude MIME types
            app.UseCompression(excludes); // Adds the SqueezeMe middleware to the pipeline, no config required
            app.UseWebApi(config);
        }
    }
}
```