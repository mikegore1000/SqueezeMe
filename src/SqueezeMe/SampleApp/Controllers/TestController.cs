using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace SampleApp.Controllers
{
    public class TestController : ApiController
    {
        [Route("test/{generateTo}")]
        public Response Get(int generateTo)
        {
            var response = new Response { Values = new List<int>()};

            for (int i = 0; i < generateTo; i++)
            {
                response.Values.Add(i);
            }

            return response;
        }
    }

    public class Response
    {
        public List<int> Values { get; set; }
    }
}
