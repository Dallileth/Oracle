using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Tests.Helpers
{
    public static class Generate
    {
        static Random random = new Random();
        public static string String(int min=5,int max=10,string chars= "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            string RandomString(int len)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, len)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            return RandomString(random.Next(min, max));
            
        }

        public static int Int(int min,int max)
        {
            return random.Next(min, max);
        }
    }
}
