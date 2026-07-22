namespace StarviaBackend.Shared.Abstractions.Exceptions;

/// <summary>Maps a caught exception to an HTTP response. Implemented in Shared.Infrastructure.</summary>
public interface IExceptionToResponseMapper
{
    ExceptionResponse Map(Exception exception);
}
