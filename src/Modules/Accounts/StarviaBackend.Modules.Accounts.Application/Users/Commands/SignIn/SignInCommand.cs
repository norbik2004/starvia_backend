using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.SignIn;

public sealed record SignInCommand(string Email, string Password) : ICommand<JsonWebToken>;
