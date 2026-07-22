namespace StarviaBackend.Shared.Abstractions.Exceptions;

/// <summary>The HTTP shape an exception maps to.</summary>
public sealed record ExceptionResponse(object Response, int StatusCode);

/// <summary>The JSON error body returned to clients.</summary>
public sealed record ErrorResponse(IReadOnlyList<Error> Errors)
{
    public static ErrorResponse Single(string code, string message) => new([new Error(code, message)]);
}

public sealed record Error(string Code, string Message);
