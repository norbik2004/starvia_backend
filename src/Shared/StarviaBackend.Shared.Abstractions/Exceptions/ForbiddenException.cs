namespace StarviaBackend.Shared.Abstractions.Exceptions;

/// <summary>
/// An authenticated caller tried to use a resource it has no right to. The error handling
/// middleware maps these to 403 (while other <see cref="BusinessException"/>s become 400).
/// </summary>
public abstract class ForbiddenException : BusinessException
{
    protected ForbiddenException(string message) : base(message)
    {
    }
}
