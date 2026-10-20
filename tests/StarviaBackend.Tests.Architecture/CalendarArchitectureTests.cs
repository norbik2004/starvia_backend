using StarviaBackend.Modules.Calendar.Api;
using ApplicationMarker = StarviaBackend.Modules.Calendar.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Calendar.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Calendar.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class CalendarArchitectureTests() : ModuleArchitectureTests(
    "Calendar",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(CalendarsModule).Assembly);
