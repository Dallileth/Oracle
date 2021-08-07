using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Box.Shared.Core.Auth
{

    public static class JWTParser
    {

        public static IEnumerable<Claim> GetClaims(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            foreach (var v in keyValuePairs)
            {
                Console.WriteLine($"{v.Key} {v.Value}");
                string str = v.Value.ToString();
                if (str[0] == '[')
                {
                    str = str.Substring(2, str.Length - 4);
                    foreach (var s in str.Split("\",\""))
                    {
                        yield return new Claim(v.Key, s);
                    }
                    //str = str.Substring(1, str.Length - 1);
                    //individual.UIAccess = str.Split(",");
                }
                else
                {
                    yield return new Claim(v.Key, v.Value.ToString());
                }

            }
        }

        static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }


    }
}
