using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sandbox.Server.Data;
using Sandbox.Server.Hubs;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Sandbox.Shared.Models;
using System;
using System.Threading.Tasks;
using Sandbox.Shared.Services;
using Sandbox.Server.Services;

namespace Sandbox.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();
            services.AddRazorPages();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateAudience = false,
                        //ValidAudience= "https://localhost", //who is this for? well, me, duh

                        ValidateIssuer = false,
                        //ValidIssuer = "https://localhost", //who issued it? well, me, duh

                        ValidateLifetime = true,
                        RequireExpirationTime=true,
                        ClockSkew=TimeSpan.FromMinutes(1), 

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = JWTHelper.Key
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(token) &&
                                (path.StartsWithSegments("/hubs/chat")))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] {
                        MediaTypeNames.Application.Octet
                    });
            });

            services.AddSignalR();
            services
                .AddScoped<IChatHub, ChatHub>()
                .AddSingleton<RestAPI>(config =>
                {
                    RestAPI.Init();
                    var api = new RestAPI(string.Empty);
                    return api;
                })
                .AddSingleton<ISQL, SQL>(
                provider =>
                {
                    return new SQL(
#if RELEASE
                        Configuration.GetConnectionString("release")
#elif STAGE
                        Configuration.GetConnectionString("stage")                        
#else
                        Configuration.GetConnectionString("dev")
#endif
                        );
                })
                .AddScoped<LoginService>(provider =>
                {
                    return new LoginService(provider.GetRequiredService<ISQL>());
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            //app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseExceptionHandler(a => a.Run(async context =>
              {
                  var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                  var exception = exceptionHandlerPathFeature.Error;
                  context.Response.ContentType = "application/json";
                  await context.Response.WriteAsync(
                      JsonConvert.SerializeObject(
                          new
                          {
#if !DEBUG
                              title = exception is ProblemException ? ((ProblemException)exception).Title : "Server Error",
                              detail = exception is ProblemException ? ((ProblemException)exception).Detail : "We were unable to process the request"
#else
                              title = exception is ProblemException ? ((ProblemException)exception).Title : "Server Error",
                              detail = exception is ProblemException ? ((ProblemException)exception).Detail : exception.Message
#endif
                          })
                            );
        }));
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapHub<ChatHub>("/hubs/chat", options =>
                {
                    //options.Transports = HttpTransportType.ServerSentEvents;
                });
            });
        }
    }
}
