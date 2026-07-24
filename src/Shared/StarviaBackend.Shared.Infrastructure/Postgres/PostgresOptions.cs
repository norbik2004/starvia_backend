namespace StarviaBackend.Shared.Infrastructure.Postgres;

public sealed class PostgresOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;
}
