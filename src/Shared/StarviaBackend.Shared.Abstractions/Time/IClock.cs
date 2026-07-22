namespace StarviaBackend.Shared.Abstractions.Time;

/// <summary>Abstracts "now" so time-dependent code is testable.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
