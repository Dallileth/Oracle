using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NUnit.Framework;
using Sandbox.Server;
using Sandbox.Server.Data;
using Sandbox.Server.Services;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using Sandbox.Shared.Services;
using Sandbox.Tests.Helpers;
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
    public class PasswordTests
    {
        LoginService _login;
        public PasswordTests()
        {
            _login = new LoginService(
                new SQL(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SandboxDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
                );
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threshold">Two strings are considered "the same" if the distance is less than this value</param>
        void AssertStringDistance(string pw1, string pw2, string message, int threshold = 30)
        {
            int ComputeDistance(string s, string t)
            {
                if (string.IsNullOrEmpty(s))
                {
                    if (string.IsNullOrEmpty(t))
                        return 0;
                    return t.Length;
                }

                if (string.IsNullOrEmpty(t))
                {
                    return s.Length;
                }

                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                // initialize the top and right of the table to 0, 1, 2, ...
                for (int i = 0; i <= n; d[i, 0] = i++) ;
                for (int j = 1; j <= m; d[0, j] = j++) ;

                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                        int min1 = d[i - 1, j] + 1;
                        int min2 = d[i, j - 1] + 1;
                        int min3 = d[i - 1, j - 1] + cost;
                        d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                    }
                }
                return d[n, m];
            }

            var distance = ComputeDistance(pw1, pw2);
            Assert.GreaterOrEqual(distance, threshold, $"{message}{Environment.NewLine}{0}{Environment.NewLine}{1}{Environment.NewLine}{2}", distance, pw1, pw2);
        }

        

        [SetUp]
        public void Setup()
        {
            ;
        }
        [TearDown]
        public void Teardown()
        {
            ;
        }


        [Test]
        public void CreateSalt_DoesNotRepeatSalts()
        {
            List<byte[]> salts = new List<byte[]>();
            for (int i = 0; i < 1000; i++)
            {
                var new_salt = _login.CreateSalt();
                foreach (var oldsalt in salts)
                {
                    Assert.False(new_salt.SequenceEqual(oldsalt), "Salts should be unique");
                }
                salts.Add(_login.CreateSalt());
            }
        }

        [Test]
        public void Passwords_WithDifferentSalts_ShouldNotBeSimilar()
        {
            for (int i = 0; i < 1000; i++)
            {
                var password = Generate.String(5,10);
                var pw1 = _login.GetHashedPassword(password, _login.CreateSalt());
                var pw2 = _login.GetHashedPassword(password, _login.CreateSalt());

                AssertStringDistance(pw1, pw2, "Password with the different salts should not be similar");
            }
        }
        

        [Test]
        public void Passwords_WithSameSalts_ShouldHaveSameHashes()
        {
            for (int i = 0; i < 100; i++)
            {
                var password = Generate.String(5,10);
                var hash = _login.CreateSalt();
                var pw1 = _login.GetHashedPassword(password, hash);
                var pw2 = _login.GetHashedPassword(password, hash);

                Assert.AreEqual(pw1, pw2, "Password with the same salt should be the same");
            }
        }

        [Test]
        public void SimilarPasswords_ShouldNotHaveSimilarHashes()
        {
            for (int i = 0; i < 1000; i++)
            {
                var testpass = Generate.String(5,10);
                var pw1 = _login.GetHashedPassword(testpass, _login.CreateSalt());
                var pw2 = _login.GetHashedPassword(testpass, _login.CreateSalt());
                Assert.AreNotEqual(pw1, pw2, "Similar passwords should not generate similar hashes");

            }
        }


        [Test]
        public void PasswordHash_FitsInsideDatabase()
        {
            for (int i = 0; i < 1000; i++)
            {
                var password = Generate.String(10,20);
                var hash = _login.GetHashedPassword(password, _login.CreateSalt());
                Assert.LessOrEqual(hash.Length, 44);
            }
        }
        [Test]
        public void PasswordSalt_FitsInsideDatabase()
        {
            var maxlen = 0;
            for (int i = 0; i < 1000; i++)
            {
                var hash = Convert.ToBase64String(_login.CreateSalt());
                var len = hash.Length;
                maxlen = Math.Max(len, maxlen);
                Assert.LessOrEqual(hash.Length, 24);
            }
        }

    }
}