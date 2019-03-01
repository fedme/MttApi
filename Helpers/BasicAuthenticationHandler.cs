using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Mtt.Helpers
{

    // IMPORTANT:
    // This authentication in insecure and for MVP testing purposes only!
    // Do not use in production!!
    // TODO
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private static string Username = "username"; // TODO: do not hardcode
        private static string Password = "password"; // TODO: do not hardcode

        public BasicAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
     
            var username = "";
            var password = "";

            try 
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                username = credentials[0];
                password = credentials[1];
            } 
            catch 
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            if ( username != Username || password != Password)
                return AuthenticateResult.Fail("Invalid Username or Password");

            var claims = new[] { 
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}