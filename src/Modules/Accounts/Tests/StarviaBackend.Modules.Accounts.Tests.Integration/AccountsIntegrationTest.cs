using System.Runtime.CompilerServices;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Accounts.Tests.Integration;

/// <summary>
/// All Accounts integration tests share one container (per collection) and run sequentially —
/// the Testcontainers connection string is passed via a process-wide env var, so parallel classes
/// would race. New test classes just derive this and belong to the same collection.
/// </summary>
[Collection(AccountsCollection.Name)]
public abstract class AccountsIntegrationTest(AccountsApp app) : IntegrationTest<AccountsApp>(app);

[CollectionDefinition(AccountsCollection.Name)]
public sealed class AccountsCollection : ICollectionFixture<AccountsApp>
{
    public const string Name = "AccountsIntegration";
}
