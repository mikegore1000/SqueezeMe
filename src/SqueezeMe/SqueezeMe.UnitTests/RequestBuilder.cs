using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System;

namespace SqueezeMe.UnitTests
{
    public class RequestBuilder
    {
        private readonly HttpRequestMessage request = new HttpRequestMessage();

        public RequestBuilder()
        {
            request.RequestUri = new Uri("http://localhost/test");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new ObjectContent<string>("Request", new JsonMediaTypeFormatter());
            request.Method = HttpMethod.Get;
        }

        public RequestBuilder WithAcceptEncoding(string acceptEncoding)
        {
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(acceptEncoding));
            return this;
        }

        public RequestBuilder WithAccept(string accept)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            return this;
        }

        public HttpRequestMessage Get()
        {
            return request;
        }
    }
}
