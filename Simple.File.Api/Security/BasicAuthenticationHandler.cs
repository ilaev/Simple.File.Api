using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Simple.File.Api.Interfaces;

namespace Simple.File.Api.Security;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserService _userService;
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IUserService userService) : base(options, logger, encoder, clock)
    {
        _userService = userService;
    }
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (authHeader != null)
            {
                var credentialsParts = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                var username  = credentialsParts[0];
                var password = credentialsParts[1];
                var isAuthenticated = _userService.ValidateCredentials(username, password);
                if (isAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, username) };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    return Task.FromResult(
                        AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                }
                return Task.FromResult(AuthenticateResult.Fail("wrong user credentials"));
            }
        }
        catch (Exception)
        {
            // ignored
        }
        return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header")); 
    }


}