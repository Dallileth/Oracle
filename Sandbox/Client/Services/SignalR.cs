using Microsoft.AspNetCore.SignalR.Client;
using Sandbox.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Sandbox.Client.Services
{
    public class SignalRVM : IAsyncDisposable
    {

        static string _uri;
        private HubConnection hubConnection;
        public string ConnectionID { get { return hubConnection.ConnectionId; } }
        public static string Auth { get; set; }
        public static void Init(string uri)
        {
            _uri = uri;
        }
        public SignalRVM(
            string hub//eg.chathub       
            )
        {
            string url = $"{_uri}/hubs/{hub}";
            hubConnection = new HubConnectionBuilder()
               .WithUrl(url, options =>
               {
                   //If you need to renew the token in order to keep the connection active (because it may expire during the connection), do so from within this function and return the updated token.
                   options.AccessTokenProvider =()=> { return Task.FromResult(Auth); };
               })
               .WithAutomaticReconnect()
               .Build();
        }
        public Task Send(string method, params object[] objects)
        {
            switch (objects.Length)
            {
                case 0:
                    return hubConnection.SendAsync(method);
                case 1:
                    return hubConnection.SendAsync(method, objects[0]);
                case 2:
                    return hubConnection.SendAsync(method, objects[0], objects[1]);
                case 3:
                    return hubConnection.SendAsync(method, objects[0], objects[1], objects[2]);
                case 4:
                    return hubConnection.SendAsync(method, objects[0], objects[1], objects[2], objects[3]);
                case 5:
                    return hubConnection.SendAsync(method, objects[0], objects[1], objects[2], objects[3], objects[4]);
                case 6:
                    return hubConnection.SendAsync(method, objects[0], objects[1], objects[2], objects[3], objects[4], objects[5]);
                case 7:
                    return hubConnection.SendAsync(method, objects[0], objects[1], objects[2], objects[3], objects[4], objects[5], objects[6]);
                default:
                    throw new ArgumentException("Invalid objects length");
            }
        }

        public SignalRVM WithMethod<T>(
            string method,
            Func<T, Task> action
            )
        {
            hubConnection.On<T>(method, action);
            return this;
        }
        public SignalRVM WithMethod(
            string method,
            Func<Task> action
            )
        {
            hubConnection.On(method, action);
            return this;
        }
        
        public async Task<SignalRVM> Complete()
        {
            await hubConnection.StartAsync();
            return this;
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
                await hubConnection.DisposeAsync();
        }
    }
}
