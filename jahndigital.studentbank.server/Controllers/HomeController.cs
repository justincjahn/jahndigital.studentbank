using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace jahndigital.studentbank.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("{userId}"), Authorize(Policy = Constants.AuthPolicy.UserDataOwner)]
        public string Get()
        {
            return "Hello, World";
        }
    }
}
