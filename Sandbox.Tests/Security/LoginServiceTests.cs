using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NUnit.Framework;
using Sandbox.Server;
using Sandbox.Server.Data;
using Sandbox.Server.Services;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using Sandbox.Shared.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Sandbox.Tests.API
{
    /// <summary>
    /// To run API Tests, start the Server without debugging
    /// </summary>
    public class LoginServiceTests
    {
        LoginService _login;
        SQL _sql;
        RestAPI _api;
        public LoginServiceTests()
        {
            _sql = new SQL(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SandboxDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            _login = new LoginService(_sql);

            _api = new RestAPI("https://localhost:44397/api", false);
        }
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _sql.Command("delete from dbo.users;select 0 as result;").FirstOrDefault();
        }
        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _sql.Command("delete from dbo.users;select 0 as result;").FirstOrDefault();
            _sql.Command("DBCC CHECKIDENT ('dbo.users', RESEED, 0)").FirstOrDefault();
            _login.CreateUser("admin", "admin", IDType.Admin);
            _login.CreateUser("user", "user", IDType.User);
            
        }

        [Test]
        [TestCase("usertest", "usertest", IDType.User)]
        [TestCase("admintest", "admintest", IDType.Admin)]
        public async Task CreateUser_CreatesUsers_ThatCanUseWebAPI(string username, string password, IDType idtype)
        {
            //setup
            var userid = _login.CreateUser(username, password, idtype);

            //try login
            
            try
            {
                var token = await _api.Post<LoginResponse>
                    (
                    "login",
                    new LoginBody
                    {
                        UserName = username,
                        Password = password,
                        Seconds = 60 * 60
                    },
                    check_auth: false);
                if (!string.IsNullOrWhiteSpace(token?.Token))
                {
                    _api.SetAuth("Bearer", token.Token, token.Expires);
                }
            }
            catch
            {
                Assert.Fail("Unable to connect login - either the website isn't running or new users can't login");
            }


            //test interactivity
            var test = await _api.Get<string>("test/returnstring",new { echo="hello motto"});
            Assert.AreEqual("hello motto", test, "Was not able to interact with the API correctly");
        }
    }
}