using NUnit.Framework;
using Sandbox.Server.Data;
using Sandbox.Server.Services;
using Sandbox.Shared.Models;
using Sandbox.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Tests
{


    [SetUpFixture]
    public class TestLifetime
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RestAPI.Init();
            var sql = new SQL(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SandboxDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            var login = new LoginService(sql);
            
            
            sql.Command("delete from dbo.users where username='admin' or username='user';select 0 as result;").FirstOrDefault();
            sql.Command("DBCC CHECKIDENT ('dbo.users', RESEED, 0)").FirstOrDefault();
            login.CreateUser("admin", "admin", IDType.Admin);
            login.CreateUser("user", "user", IDType.User);
           
        }

        [OneTimeTearDown]
        public void Teardown()
        {

        }
    }
}
