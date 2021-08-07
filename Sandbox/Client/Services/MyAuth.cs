using Box.Shared.Core.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Sandbox.Shared.Models;
using Sandbox.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sandbox.Client.Services
{

    public class MyAuth : AuthenticationStateProvider
    {
        static ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        public ClaimsPrincipal Auth { get; private set; } = _anonymous;
        public Individual Me { get; private set; } = null;

        RestAPI _api;
        public MyAuth(RestAPI api)
        {
            _api = api;
        }

        public void Update()
        {
            this.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsPrincipal user = _anonymous;
            var jwt = _api.Auth?.Parameter;
            if (!string.IsNullOrWhiteSpace(jwt))
            {
                var claims = JWTParser.GetClaims(jwt);
#if DEBUG
                var enum_claims = claims.ToArray();
#endif
                user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role));
            }
            Me = user == _anonymous ? null : user.ToIndividual();
            Auth = user;

            return new AuthenticationState(Auth);
        }
    }
}
