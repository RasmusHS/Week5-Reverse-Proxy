using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Week5_Reverse_Proxy.Auth;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the BasicAuthHandler class with the specified authentication options, logger
    /// factory, URL encoder, and configuration settings.
    /// </summary>
    /// <param name="options">The monitor that provides the authentication scheme options used to configure the handler.</param>
    /// <param name="logger">The factory used to create logger instances for logging within the handler.</param>
    /// <param name="encoder">The encoder used to encode URLs and related data for authentication processing.</param>
    /// <param name="configuration">The configuration instance that supplies application settings required by the handler.</param>
    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration) : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Attempts to authenticate the current request using the Basic authentication scheme.
    /// </summary>
    /// <remarks>This method expects the 'Authorization' header to be present in the request and formatted
    /// according to the Basic authentication scheme. Authentication succeeds only if the provided credentials match the
    /// configured username and password. If authentication fails, the result includes a failure reason that can be used
    /// for diagnostics.</remarks>
    /// <returns>A task that represents the asynchronous authentication operation. The task result contains an <see
    /// cref="AuthenticateResult"/> indicating whether authentication was successful, failed due to missing or invalid
    /// credentials, or failed due to an unsupported authentication scheme.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the Authorization header is present in the request
        if (!Request.Headers.ContainsKey("Authorization")) 
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));

        // Retrieve the value of the Authorization header as a string
        var authHeader = Request.Headers.Authorization.ToString(); 

        // Check if the Authorization header starts with "Basic " (case-insensitive).
        // If it doesn't, return a failure result indicating an invalid authentication scheme.
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) 
            return Task.FromResult(AuthenticateResult.Fail("Invalid scheme"));

        // Extract the Base64-encoded credentials from the Authorization header by removing the "Basic " prefix,
        // decode it from Base64 to a byte array, and then convert it to a UTF-8 string.
        // The resulting string is expected to be in the format "username:password".
        var credentialBytes = Convert.FromBase64String(authHeader["Basic ".Length..]);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

        // Check if the credentials are in the expected format (i.e., contain both username and password).
        var username = credentials[0];
        var password = credentials[1];

        // Retrieve the expected username and password from the application configuration settings.
        var expectedUser = _configuration["AdminAuth:Username"];
        var expectedPass = _configuration["AdminAuth:Password"];

        // Compare the provided username and password with the expected values from the configuration.
        if (username != expectedUser || password != expectedPass)
            return Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));

        // If the credentials are valid, create a set of claims for the authenticated user, including a claim for the user's name.
        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // Return a successful authentication result containing the authentication ticket,
        // which includes the user's claims and the authentication scheme.
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// This method is to demonstrate a basic authentication and authorization implementation, using the browser to prompt for details instead of making a login screen.
    /// Handles an authentication challenge by setting the HTTP Basic authentication header and invoking the base
    /// challenge handling logic.
    /// </summary>
    /// <remarks>Sets the 'WWW-Authenticate' header in the HTTP response to prompt the client for Basic
    /// authentication credentials. This method is typically called when authentication is required but has not been
    /// provided.</remarks>
    /// <param name="properties">The authentication properties associated with the current challenge request. May contain redirect URIs or other
    /// state information relevant to the authentication process.</param>
    /// <returns>A task that represents the asynchronous operation of handling the authentication challenge.</returns>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = "Basic realm=\"Admin\"";
        return base.HandleChallengeAsync(properties);
    }
}
