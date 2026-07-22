namespace StarviaBackend.Shared.Abstractions.Exceptions;

/// <summary>
/// Base for expected, domain-level failures. The error handling middleware maps these
/// to 400-family responses using <see cref="Code"/>. Unmapped exceptions become 500.
/// </summary>
public abstract class BusinessException : Exception
{
    /// <summary>Stable, machine-readable error code (e.g. "email_already_in_use").</summary>
    public abstract string Code { get; }

    protected BusinessException(string message) : base(message)
    {
    }
}
