using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Shared.ControllerModels
{

    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime? Expires { get; set; }
    }

    public class LoginBody
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Seconds { get; set; }
    }

    public class RegisterBody
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public IDType IDType { get; set; }
    }

}
