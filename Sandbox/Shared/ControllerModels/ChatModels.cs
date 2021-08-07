using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Shared.ControllerModels
{
        public class ChatBody
        {
            public string Message { get; set; }
            public string ConnectionId { get; set; }
        }
    public class ChatUser
    {
        public string User { get; set; }
        public string ConnectionId { get; set; }
    }
    
}
