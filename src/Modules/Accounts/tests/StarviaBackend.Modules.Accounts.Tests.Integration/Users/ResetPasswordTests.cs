using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class ResetPasswordTests(AccountsApp app) : AccountsIntegrationTest(app)
{
    [Fact]
    public async Task Forgot_password_returns_ok_for_existing_and_unknown_emails()
    {
        var email = UniqueEmail();
        await RegisterAndConfirm(email);

        (await Client.PostAsJsonAsync("/v1/accounts/forgot-password", new { email }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        (await Client.PostAsJsonAsync("/v1/accounts/forgot-password", new { email = UniqueEmail() }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Reset_password_with_valid_code_allows_sign_in_with_new_password()
    {
        var email = UniqueEmail();
        var userId = await RegisterAndConfirm(email);
        var code = await GetPasswordResetCode(userId);

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/reset-password",
            new { userId, code, password = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        (await Client.PostAsJsonAsync("/v1/accounts/sign-in", new { email, password = "Passw0rd!" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var signIn = await Client.PostAsJsonAsync(
            "/v1/accounts/sign-in",
            new { email, password = "N3wPassw0rd!" });
        signIn.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await signIn.Content.ReadFromJsonAsync<JsonWebToken>();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Reset_password_with_invalid_code_returns_bad_request()
    {
        var email = UniqueEmail();
        var userId = await RegisterAndConfirm(email);

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/reset-password",
            new { userId, code = "not-a-valid-code", password = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == "invalid_password_reset_code");
    }

    private async Task<Guid> Register(string email)
    {
        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/register",
            new { email, password = "Passw0rd!", userName = email });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        return body!.UserId;
    }

    private async Task<Guid> RegisterAndConfirm(string email)
    {
        var userId = await Register(email);
        await UsingServicesAsync(async sp =>
        {
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
            (await userManager.ConfirmEmailAsync(user!, token)).Succeeded.Should().BeTrue();
        });
        return userId;
    }

    private async Task<string> GetPasswordResetCode(Guid userId)
    {
        return await UsingServicesAsync(async sp =>
        {
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            var token = await userManager.GeneratePasswordResetTokenAsync(user!);
            return PasswordResetLink.EncodeToken(token);
        });
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private sealed record RegisterUserResponse(Guid UserId);
}
