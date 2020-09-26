using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace jahndigital.studentbank.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public string Get([FromServices] AppDbContext context)
        {
            var admin = context.Users.Where(x => x.Email == "admin@domain.tld").FirstOrDefault();
            var valid = admin.ValidatePassword("admin");

            admin.Password = "hello";
            context.SaveChanges();
            var valid2 = admin.ValidatePassword("hello");

            return $"{valid} {valid2}";
        }
    }
}
