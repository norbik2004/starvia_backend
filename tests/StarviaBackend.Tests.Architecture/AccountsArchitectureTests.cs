using StarviaBackend.Modules.Accounts.Api;
using ApplicationMarker = StarviaBackend.Modules.Accounts.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Accounts.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Accounts.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class AccountsArchitectureTests() : ModuleArchitectureTests(
    "Accounts",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(AccountsModule).Assembly);
