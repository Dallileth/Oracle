using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Sandbox.Server.Hubs;
using Sandbox.Server.Services;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sandbox.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        LoginService _login;
        public LoginController(LoginService login)
        {
            _login = login;
        }

        [HttpPost()]
        public LoginResponse Login([FromBody]LoginBody body)
        {
            try
            {
                return _login.Login(body);
            }
            catch(Exception exc)
            {
                throw new ProblemException("Incorrect username or password");
            }
        }


        [HttpPut]
        public LoginResponse Register([FromBody]RegisterBody body)
        {
            _login.CreateUser(body.UserName, body.Password, body.IDType);
            return _login.Login(new LoginBody
            {
                Password = body.Password,
                UserName = body.UserName,
                Seconds = 60 * 60
            });
        }

    }
}
