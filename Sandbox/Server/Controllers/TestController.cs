using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sandbox.Server.Data;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sandbox.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        
        // start problems
        

        [HttpGet("RequiresDNERole")]
        [Authorize(Roles = "zzz")]
        public IActionResult RequiresDNERole()
        {
            return Ok();
        }

        [HttpGet("timeout")]
        public async Task<string> Timeout()
        {
            await Task.Delay(20000);
            return "Super long data";
        }

        [HttpGet("forbidden")]
        public IActionResult Forbidden()
        {
            return Forbid();
        }



        [HttpGet("problemexception")]
        public LoginResponse ProblemException()
        {
            throw new ProblemException(
                "ProblemException Detail"
                );
        }

        [HttpGet("problemreturn")]
        public IActionResult ProblemReturn()
        {
            return Problem(
                title: "Problem Title",
                detail: "Problem Detail");
        }


        [HttpGet("DivideByZero")]
        public string DivideByZero()
        {
            int a = 0;
            return (3 / a).ToString();
        }

        // end problems



        [HttpGet("returnok")]
        public IActionResult ReturnOk()
        {
            return Ok();
        }


        [HttpGet("returnstring")]
        public IActionResult ReturnString([FromQuery]string echo)
        {
            return Ok(echo);
        }

        [HttpGet("returnint")]
        public IActionResult ReturnInt()
        {
            return Ok(3);
        }
        [HttpGet("returndate")]
        public IActionResult ReturnDate()
        {
            return Ok(DateTime.Now);
        }

        [HttpGet("returnints")]
        public IEnumerable<int> ReturnInts()
        {
            return Enumerable.Range(5,5);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("ReturnAdminData")]
        public IActionResult ReturnAdminData()
        {
            return Ok("Super secret data");
        }

        [HttpGet("returnuserdata")]
        [Authorize(Roles = "User, Admin")]
        public IActionResult ReturnNormalData()
        {
            return Ok("Generic staff data");
        }


        static Random r = new Random();

        [HttpGet("add")]
        [Authorize(Roles ="Admin")]
        public async Task<string> Add([FromQuery]int a,[FromQuery]int b,[FromQuery]int error)
        {
            await Task.Delay(200 + r.Next(4000));
            if (error >= r.Next(100))
                throw new ProblemException("API decided to throw");
            else
                return (a + b).ToString();
        }


    }
}
