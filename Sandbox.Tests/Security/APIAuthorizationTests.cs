using NUnit.Framework;
using Sandbox.Server;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using Sandbox.Shared.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sandbox.Tests.API
{
    public class APIAuthorizationTests
    {
        RestAPI _api;

        public APIAuthorizationTests()
        {
            _api = new RestAPI("https://localhost:44397/api", false);
        }


        [TearDown]
        public void Teardown()
        {
            _api.ClearAuth();
        }

        [Test]
        [TestCase(IDType.Admin, true)]
        [TestCase(IDType.User, false)]
        [TestCase(null, false)]
        public async Task AdminEndpoints_AreOnlyAccessibleByAdmins(IDType? idtype, bool expect_ok)
        {
            if (idtype != null)
            {
                var token = await _api.Post<LoginResponse>(
                    "login", 
                    new LoginBody 
                    {  
                        Seconds = 40, 
                        UserName = idtype.Value.ToString().ToLower(),
                        Password= idtype.Value.ToString().ToLower()
                    }, 
                    check_auth: false);

                _api.SetAuth("Bearer", token.Token, token.Expires);
            }

            var result = await _api.Get<HttpStatusCode>("test/ReturnAdminData", check_auth: false);
            if (expect_ok)
                Assert.AreEqual(HttpStatusCode.OK, result);
            else
                Assert.AreNotEqual(HttpStatusCode.OK, result);
        }


        [Test]
        [TestCase(IDType.Admin)]

        [TestCase(IDType.User)]
        public async Task Endpoints_CanSupportMultipleRoles(IDType idtype)
        {
                var token = await _api.Post<LoginResponse>(
                    "login",
                    new LoginBody
                    {
                        Seconds = 20,
                        UserName = idtype.ToString().ToLower(),
                        Password = idtype.ToString().ToLower()
                    },
                    check_auth: false);

                _api.SetAuth("Bearer", token.Token, token.Expires);
            var result = await _api.Get<HttpStatusCode>("test/ReturnNormalData", check_auth: false);
            Assert.AreEqual(HttpStatusCode.OK, result);
        }


        /// <summary>
        /// Note: ClockSkew is 1 minute. 
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task LoginTokens_CanExpire()
        {
            bool accessed_while_authorized;
            bool accessed_while_expired;

            var token = await _api.Post<LoginResponse>(
                "login", 
                new LoginBody 
                { 
                    Seconds = 2, 
                    UserName = "admin",
                    Password="admin"
                }, check_auth: false);
            _api.SetAuth("Bearer", token.Token, token.Expires);

            var result = await _api.Get<HttpStatusCode>("test/ReturnAdminData");
            accessed_while_authorized = result == HttpStatusCode.OK;
            await Task.Delay(1000 * 60 * 3); //3 minutes

            var result2 = await _api.Get<HttpStatusCode>("test/ReturnAdminData", check_auth: false);
            accessed_while_expired = result2 == HttpStatusCode.OK;

            Assert.True(accessed_while_authorized, "Access while authorized");
            Assert.False(accessed_while_expired, "Access while unauthorized");
        }

        [Test]
        public async Task LoginTokens_CantBeModified()
        {
            var token = await _api.Post<LoginResponse>(
                "login", 
                new LoginBody 
                {                     
                    Seconds = 60*10, 
                    UserName = IDType.User.ToString().ToLower(),
                    Password= IDType.User.ToString().ToLower()

                }, 
                check_auth: false);
            
            var handler = new JwtSecurityTokenHandler();
            var newtoken = handler.ReadJwtToken(token.Token);
            var claims = newtoken.Claims.Append(new Claim(ClaimTypes.Role, "Admin")).ToArray();

            var spoofed_token=JWTHelper.CreateToken(TimeSpan.FromHours(24),claims);

            _api.SetAuth("Bearer", spoofed_token);
            var result = await _api.Get<HttpStatusCode>("test/ReturnAdminData",check_auth:false);

            Assert.AreNotEqual(HttpStatusCode.OK, result);

        }

        [Test]
        public void LoginTokens_CantBeIntercepted()
        {
            var using_https = _api.BaseAddress.StartsWith(@"https://");
            Assert.True(using_https, "Endpoints are using https");

            using (var httpapi = new RestAPI("http://localhost:44397/api", false))
            {
                Assert.That(
                    async () =>
                    {
                        var token = await httpapi.Post<LoginResponse>("login", new LoginBody { Seconds = 6, UserName = "admin",Password="admin" }, check_auth: false);
                        httpapi.SetAuth("Bearer", token.Token, token.Expires);

                        var result = await httpapi.Get<HttpStatusCode>("test/ReturnAdminData", check_auth: false);
                        Assert.AreNotEqual(HttpStatusCode.OK, result);
                    },
                    Throws.Exception,
                    "Endpoints can't be accessed via http"
                    );

            }
        }
    }
}