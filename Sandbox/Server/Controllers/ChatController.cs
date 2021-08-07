using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sandbox.Server.Hubs;
using Sandbox.Shared.ControllerModels;
using Sandbox.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sandbox.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        IHubContext<ChatHub> _chat;
        public ChatController(IHubContext<ChatHub> chat)
        {
            _chat = chat;
        }
        static Random r = new Random();

        [Authorize()]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody]ChatBody  body)
        {
            await Task.Delay(600);
            if (r.Next(100) < 85)
            {
                await _chat.Clients.AllExcept(body.ConnectionId).SendAsync("addmessage", new ChatBody  { ConnectionId=HttpContext.User.Identity.Name,Message=body.Message});
                return Ok();
            }
            else
                return Problem(detail:"I stopped the message from coming through",title:"Chat Error");
            
        }

    }
}
