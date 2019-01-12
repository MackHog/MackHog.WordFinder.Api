using Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebApp.Config
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AppSettings _appSettings;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            AppSettings appSettings)
            : base(options, logger, encoder, clock)
        {
            _appSettings = appSettings;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Request.Headers.ContainsKey("Authorization"))
                    return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                if (string.IsNullOrEmpty(authHeader.Parameter))
                    return await Task.FromResult(AuthenticateResult.Fail("Missing Authorization Parameter"));

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                if (credentials.Length < 2)
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Parameter"));

                var username = credentials[0];
                var password = credentials[1];

                if (!string.Equals(username, _appSettings.Username) || !string.Equals(password, _appSettings.Password))
                    return AuthenticateResult.Fail("Invalid Username or Password");

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, Constants.Policy.Read),
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                return await Task.FromResult(AuthenticateResult.Fail("Authentication failed"));
            }
        }
    }
}
