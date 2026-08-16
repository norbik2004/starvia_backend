using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Core.Users.Exceptions;

internal sealed class EmailAlreadyInUseException(string email)
    : BusinessException($"Email '{email}' is already in use.")
{
    public override string Code => "email_already_in_use";
}

internal sealed class UserNotFoundException(Guid userId)
    : BusinessException($"User '{userId}' was not found.")
{
    public override string Code => "user_not_found";
}

internal sealed class InvalidCredentialsException()
    : BusinessException("Invalid email or password.")
{
    public override string Code => "invalid_credentials";
}

internal sealed class UserCreationFailedException(string reason)
    : BusinessException($"Could not create the account: {reason}")
{
    public override string Code => "user_creation_failed";
}

internal sealed class EmailNotConfirmedException()
    : BusinessException("The email address has not been confirmed.")
{
    public override string Code => "email_not_confirmed";
}

internal sealed class EmailArleadyConfirmedException(string email)
    : BusinessException($"Email {email} is arleady confirmed")
{
    public override string Code => "email_arleady_confirmed";
}

internal sealed class InvalidEmailConfirmationCodeException()
    : BusinessException("The email confirmation code is invalid or has expired.")
{
    public override string Code => "invalid_email_confirmation_code";
}
