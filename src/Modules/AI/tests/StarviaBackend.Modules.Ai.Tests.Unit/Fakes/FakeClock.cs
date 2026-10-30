using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

internal sealed class FakeClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; set; } = utcNow;
}
