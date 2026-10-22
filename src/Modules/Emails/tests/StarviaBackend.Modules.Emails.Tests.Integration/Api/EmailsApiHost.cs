using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarviaBackend.Modules.Emails.Api;
using StarviaBackend.Modules.Emails.Tests.Integration.Support;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Infrastructure.Api;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Api;

/// <summary>
/// A minimal in-memory web host that mounts only the Emails Api assembly: real routing, model
/// binding, authentication/authorization and JSON, with a recording <see cref="IDispatcher"/>
/// behind the endpoints. Avoids the Bootstrapper (and so every other module).
/// </summary>
public sealed class EmailsApiHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private EmailsApiHost(WebApplication app, RecordingDispatcher dispatcher)
    {
        _app = app;
        Dispatcher = dispatcher;
    }

    internal RecordingDispatcher Dispatcher { get; }

    internal static async Task<EmailsApiHost> StartAsync()
    {
        var dispatcher = new RecordingDispatcher();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddSingleton<IDispatcher>(dispatcher);
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(EmailsModule).Assembly)
            .ConfigureApplicationPartManager(manager =>
                manager.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        builder.Services
            .AddAuthentication(HeaderAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthHandler>(HeaderAuthHandler.SchemeName, _ => { });
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        return new EmailsApiHost(app, dispatcher);
    }

    /// <summary>A client acting as the given user (<c>X-Test-UserId</c>); anonymous when <paramref name="userId"/> is null.</summary>
    public HttpClient CreateClient(Guid? userId = null, params string[] roles)
    {
        var client = _app.GetTestClient();

        if (userId is not null)
        {
            client.DefaultRequestHeaders.Add(HeaderAuthHandler.UserIdHeader, userId.Value.ToString());
            client.DefaultRequestHeaders.Add(HeaderAuthHandler.RolesHeader, string.Join(',', roles));
        }

        return client;
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    private sealed class HeaderAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "HeaderAuth";
        public const string UserIdHeader = "X-Test-UserId";
        public const string RolesHeader = "X-Test-Roles";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserIdHeader, out var userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim> { new(UserClaims.UserId, userId.ToString()) };

            if (Request.Headers.TryGetValue(RolesHeader, out var roles))
            {
                claims.AddRange(roles.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(role => new Claim(UserClaims.Role, role)));
            }

            var identity = new ClaimsIdentity(claims, SchemeName, UserClaims.UserId, UserClaims.Role);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
