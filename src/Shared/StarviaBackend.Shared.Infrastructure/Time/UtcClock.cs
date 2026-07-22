using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Shared.Infrastructure.Time;

internal sealed class UtcClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
