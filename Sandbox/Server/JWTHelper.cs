using Microsoft.IdentityModel.Tokens;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Server
{
    public static class JWTHelper
    {
        static JWTHelper()
        {
            using (RandomNumberGenerator rng=new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[256];
                rng.GetBytes(data);
                var secret = Convert.ToBase64String(data);
                Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            }
        }
        public static SecurityKey Key { get; private set; }
        public static string CreateToken(Individual individual, TimeSpan expires)
            => CreateToken(expires, individual.GetClaims());

        public static string CreateToken(TimeSpan expires, params Claim[] claims)
        {

            var token = new JwtSecurityToken(
                null, null,
                claims,
                null, DateTime.UtcNow.Add(expires),
                new SigningCredentials(Key, SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
