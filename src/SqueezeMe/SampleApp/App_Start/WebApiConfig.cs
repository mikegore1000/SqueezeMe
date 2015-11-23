using SqueezeMe;
using System.Web.Http;

namespace SampleApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Insert(0, new CompressionHandler());

            config.MapHttpAttributeRoutes();
        }
    }
}
