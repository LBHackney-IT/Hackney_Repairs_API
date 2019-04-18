using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Controllers
{
    [Route("v1/test")]
    public class TestController : Controller
    {
        [HttpGet]
        public async Task<string> something()
        {
            return "hi";
        }
    }
}
