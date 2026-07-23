using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace StarviaBackend.Shared.Tests.Integration;

/// <summary>
/// Base class for integration tests. Provides an <see cref="HttpClient"/> authenticated as a
/// default user, helpers to seed/assert DB state, and a way to act as arbitrary roles.
/// </summary>
public abstract class IntegrationTest<TApp> : IClassFixture<TApp>
    where TApp : BoilerplateApp
{
    private readonly TApp _app;

    protected HttpClient Client { get; }

    protected IntegrationTest(TApp app)
    {
        _app = app;
        Client = app.CreateClient();
        Client.DefaultRequestHeaders.Add(FakeAuthHandler.UserIdHeader, Guid.NewGuid().ToString());
        Client.DefaultRequestHeaders.Add(FakeAuthHandler.RolesHeader, "User");
    }

    /// <summary>Builds a client acting as a specific user with the given roles.</summary>
    protected HttpClient CreateClient(Guid userId, params string[] roles)
    {
        var client = _app.CreateClient();
        client.DefaultRequestHeaders.Add(FakeAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(FakeAuthHandler.RolesHeader, string.Join(',', roles));
        return client;
    }

    /// <summary>Runs an action against a fresh scope of <typeparamref name="TDb"/> for seeding/asserting.</summary>
    protected async Task WithDbAsync<TDb>(Func<TDb, Task> action)
        where TDb : DbContext
    {
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDb>();
        await action(db);
    }

    protected async Task<TResult> WithDbAsync<TDb, TResult>(Func<TDb, Task<TResult>> func)
        where TDb : DbContext
    {
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDb>();
        return await func(db);
    }
}
