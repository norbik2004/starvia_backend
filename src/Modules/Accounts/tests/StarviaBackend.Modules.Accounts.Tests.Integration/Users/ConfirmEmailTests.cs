using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class ConfirmEmailTests(AccountsApp app) : AccountsIntegrationTest(app)
{
    [Fact]
    public async Task Confirm_email_with_valid_code_sets_confirmed()
    {
        var email = UniqueEmail();
        var userId = await Register(email);
        var code = await GetConfirmationCode(userId);

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/confirm-email",
            new { userId, code });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await WithDbAsync<AccountsWriteDbContext>(async db =>
        {
            var user = await db.Users.SingleAsync(u => u.Id == userId);
            user.EmailConfirmed.Should().BeTrue();
        });
    }

    [Fact]
    public async Task Confirm_email_is_idempotent_when_already_confirmed()
    {
        var email = UniqueEmail();
        var userId = await Register(email);
        var code = await GetConfirmationCode(userId);
        var request = new { userId, code };

        (await Client.PostAsJsonAsync("/v1/accounts/confirm-email", request))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        (await Client.PostAsJsonAsync("/v1/accounts/confirm-email", request))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Confirm_email_with_invalid_code_returns_bad_request()
    {
        var email = UniqueEmail();
        var userId = await Register(email);

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/confirm-email",
            new { userId, code = "not-a-valid-code" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == "invalid_email_confirmation_code");

        await WithDbAsync<AccountsWriteDbContext>(async db =>
        {
            var user = await db.Users.SingleAsync(u => u.Id == userId);
            user.EmailConfirmed.Should().BeFalse();
        });
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

    private async Task<string> GetConfirmationCode(Guid userId)
    {
        return await UsingServicesAsync(async sp =>
        {
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
            return EmailConfirmationLink.EncodeToken(token);
        });
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private sealed record RegisterUserResponse(Guid UserId);
}
