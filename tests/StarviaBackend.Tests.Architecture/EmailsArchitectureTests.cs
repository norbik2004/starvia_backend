using StarviaBackend.Modules.Emails.Api;
using ApplicationMarker = StarviaBackend.Modules.Emails.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Emails.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Emails.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class EmailsArchitectureTests() : ModuleArchitectureTests(
    "Emails",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(EmailsModule).Assembly);
