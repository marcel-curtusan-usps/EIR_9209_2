   using Microsoft.AspNetCore.Authentication;
   using Microsoft.Extensions.Logging;
   using Microsoft.Extensions.Options;
   using System.Net.Http.Headers;
   using System.Security.Claims;
   using System.Text;
   using System.Text.Encodings.Web;

   public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
   {
    private readonly IConfiguration _configuration;
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        IConfiguration configuration,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            var password = credentials[1];
            string allowedUsernameCredentials = _configuration["ApplicationConfiguration:APIUserCredentials"] ?? throw new ArgumentNullException("APIUserCredentials");
            string allowedUsername = _configuration["ApplicationConfiguration:APIUser"] ?? throw new ArgumentNullException("APIUser");
            // Validate the username and password here
            if (username != allowedUsername || password != allowedUsernameCredentials)
            {
                return AuthenticateResult.Fail("Invalid Username or Password");
            }

            var claims = new[] {
                   new Claim(ClaimTypes.Name, username)
               };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
    }
   }
   