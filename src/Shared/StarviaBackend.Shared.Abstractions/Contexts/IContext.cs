namespace StarviaBackend.Shared.Abstractions.Contexts;

/// <summary>Per-request ambient context: correlation id + authenticated identity.</summary>
public interface IContext
{
    string RequestId { get; }
    string CorrelationId { get; }
    string? TraceId { get; }
    IIdentityContext Identity { get; }
}
