using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;
using Testcontainers.PostgreSql;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Persistence;

/// <summary>
/// Throwaway Postgres (Testcontainers) with the Emails migrations applied. The module's DbContexts
/// are built directly, so no host (and therefore no other module) is involved.
/// </summary>
public sealed class EmailsDatabase : IAsyncLifetime
{
    public const string CollectionName = "Emails database";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateWriteContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    internal EmailsWriteDbContext CreateWriteContext() =>
        new(new DbContextOptionsBuilder<EmailsWriteDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options);

    internal EmailsReadDbContex CreateReadContext() =>
        new(new DbContextOptionsBuilder<EmailsReadDbContex>()
            .UseNpgsql(_container.GetConnectionString())
            .Options);

    /// <summary>Empties the table so every test starts from a clean slate.</summary>
    public async Task ResetAsync()
    {
        await using var db = CreateWriteContext();
        await db.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {EmailsWriteDbContext.Schema}.\"Emails\"");
    }
}

[CollectionDefinition(EmailsDatabase.CollectionName)]
public sealed class EmailsDatabaseCollection : ICollectionFixture<EmailsDatabase>;
