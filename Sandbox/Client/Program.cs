using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using Sandbox.Client.Services;
using Sandbox.Shared.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            
            builder.Services.AddOptions()
                .AddAuthorizationCore()
                .AddSingleton<AuthenticationStateProvider, MyAuth>()
                .AddSingleton<MyAuth>(provider => (MyAuth)provider.GetRequiredService<AuthenticationStateProvider>())
                .AddSingleton<RestAPI>(provider =>
                {
                    SignalRVM.Init(GetBaseURI(builder));
                    RestAPI.Init();
                    var API = new RestAPI($"{GetBaseURI(builder)}/api");
                    API.OnAuthChanged += async (o, auth) =>
                     {
                         var nav = provider.GetRequiredService<NavigationManager>();
                         var js = provider.GetRequiredService<IJSRuntime>();                         
                         if (auth == null)
                         {
                             SignalRVM.Auth = null;
                             provider.GetRequiredService<MyAuth>().Update();
                             nav.NavigateTo("login");
                             await js.InvokeVoidAsync("cookies.set", "jwt", null, null);
                             await js.InvokeVoidAsync("cookies.set", "expires", null,null);
                         }
                         else
                         {
                             SignalRVM.Auth = auth.Parameter;
                             provider.GetRequiredService<MyAuth>().Update();
                             //nav.NavigateTo("/");
                             await js.InvokeVoidAsync("cookies.set", "jwt", auth.Parameter, auth.Expires.Value);
                             await js.InvokeVoidAsync("cookies.set", "expires", auth.Expires.Value, auth.Expires.Value);
                         }                          
                     };

                    return API;
                })
                .AddMudServices(sconfig =>
                {
                    var config = sconfig.SnackbarConfiguration;
                    config.PreventDuplicates = false;
                    config.NewestOnTop = false;
                    config.MaximumOpacity = 98;
                    config.ShowCloseIcon = true;
                    config.VisibleStateDuration = 15000;
                    config.HideTransitionDuration = 100;
                    config.ShowTransitionDuration = 250;
                    config.SnackbarVariant = Variant.Outlined;
                    config.PositionClass = Defaults.Classes.Position.BottomCenter;
                });
            await builder.Build().RunAsync();
        }
        static string GetBaseURI(WebAssemblyHostBuilder builder)
        {
            var uri = new Uri(builder.HostEnvironment.BaseAddress).ToString();
            uri = uri.Substring(0, uri.Length - 1);
            return uri;
        }

    }
}
