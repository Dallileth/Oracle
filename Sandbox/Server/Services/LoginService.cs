using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Sandbox.Server.Data;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Sandbox.Server.Services
{
    public class LoginService
    {
        ISQL _sql;
        public LoginService(ISQL sql)
        {
            _sql = sql;
        }

        public byte[] CreateSalt()
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt_bytes = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt_bytes);
            }
            return salt_bytes;
        }
        public string GetHashedPassword(string password, byte[] salt_bytes)
        {
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed_password = Convert.ToBase64String
                (
                    KeyDerivation.Pbkdf2
                    (
                        password: password,
                        salt: salt_bytes,
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8
                    )
                );
            return hashed_password;
        }

        public LoginResponse Login(LoginBody body)
        {
            string username = body.UserName;
            string password = body.Password;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ProblemException("Username and password required to register");
            }
            if (username.Length > 20)
            {
                throw new ProblemException("Username cannot exceed 20 characters");
            }

            var results =
                _sql.Command(
                        "select id,salt from dbo.users where username=@username",
                        new
                        {
                            username = body.UserName
                        }
                    ).FirstOrDefault();

            var id = (int)results["id"];
            var salt = (string)results["salt"];

            var login= _sql.StoredProc("dbo.login", new
            {
                id = id,
                passwordhash = GetHashedPassword
                    (
                    body.Password
                    , Convert.FromBase64String(salt)
                    )
            }).FirstOrDefault();

            var idtype = (IDType)(Int16)login["IDType"];
            return new LoginResponse
            {
                Token =
                    JWTHelper.CreateToken(
                        new Individual
                        {
                            ID = id,
                            IDType = idtype,
                            Name = body.UserName,
                            Email="noemail@noemail.com",
                            UIAccess = idtype.GetAccess()
                        },
                        TimeSpan.FromSeconds(body.Seconds)
                    ),
                Expires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(body.Seconds))
            };
        }

        public int CreateUser(string username,string password,IDType idtype)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ProblemException("Username and password required to register");
            }
            if (username.Length>20)
            {
                throw new ProblemException("Username cannot exceed 20 characters");
            }
            var salt_bytes = CreateSalt();
            var salt = Convert.ToBase64String(salt_bytes);
            var passwordhash = GetHashedPassword(password, salt_bytes);
            var row = 
                _sql.StoredProc(
                "dbo.login_CreateUser",
                    new
                    {
                        username = username,
                        passwordhash = passwordhash,
                        salt = salt,
                        idtype = idtype
                    }).FirstOrDefault();
            var newid = (int?)row["ID"];
            if (newid is null)
                throw new ProblemException("Username already exists");
            return newid.Value;
        }
    }
}
