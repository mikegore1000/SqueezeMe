using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SqueezeMe.UnitTests.Controllers
{
    public class FakeController : ApiController
    {
        [Route("test")]
        public string Get()
        {
            return "Test";
        }
    }
}
