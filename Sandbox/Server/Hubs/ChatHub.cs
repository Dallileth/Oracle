using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Sandbox.Shared.ControllerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sandbox.Server.Hubs
{
    [Authorize]
    public class ChatHub : Hub, IChatHub
    {
        /// <summary>
        /// Note: Use a database
        /// </summary>
        static Dictionary<string, ChatUser> users = new Dictionary<string, ChatUser>();

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("hello", users.Values.ToArray());
            var me = new ChatUser
            {
                User = Context.User.Identity.Name,
                ConnectionId = Context.ConnectionId
            };
            users.Add(me.ConnectionId, me);
            await Clients.All.SendAsync("hello",new ChatUser[] { me});
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {            
            users.Remove(Context.ConnectionId);
            await Clients.Others.SendAsync("bye", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

    }
    public interface IChatHub
    {
    }
}
