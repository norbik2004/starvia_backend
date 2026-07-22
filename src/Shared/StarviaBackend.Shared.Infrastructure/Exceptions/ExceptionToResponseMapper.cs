using System.Net;
using FluentValidation;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Shared.Infrastructure.Exceptions;

internal sealed class ExceptionToResponseMapper : IExceptionToResponseMapper
{
    public ExceptionResponse Map(Exception exception) => exception switch
    {
        ValidationException validation => new ExceptionResponse(
            new ErrorResponse(validation.Errors
                .Select(e => new Error(ToCode(e.PropertyName), e.ErrorMessage))
                .ToArray()),
            (int)HttpStatusCode.BadRequest),

        BusinessException business => new ExceptionResponse(
            ErrorResponse.Single(business.Code, business.Message),
            (int)HttpStatusCode.BadRequest),

        _ => new ExceptionResponse(
            ErrorResponse.Single("error", "There was an error processing your request."),
            (int)HttpStatusCode.InternalServerError),
    };

    private static string ToCode(string propertyName) =>
        string.IsNullOrWhiteSpace(propertyName) ? "invalid" : propertyName.Underscore();
}

internal static class StringExtensions
{
    public static string Underscore(this string value) =>
        string.Concat(value.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}
