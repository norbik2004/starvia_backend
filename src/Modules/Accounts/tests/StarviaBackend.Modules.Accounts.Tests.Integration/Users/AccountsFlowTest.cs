using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Shared.Infrastructure.Auth;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

/// <summary>Shared arrange helpers for tests that need real accounts in the database.</summary>
public abstract class AccountsFlowTest(AccountsApp app) : AccountsIntegrationTest(app)
{
    protected const string Password = "Passw0rd!";

    protected static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    /// <summary>A client that sends no identity at all (anonymous caller).</summary>
    protected HttpClient CreateAnonymousClient() => App.CreateClient();

    protected async Task<Guid> RegisterAsync(string? email = null)
    {
        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/register",
            new { email = email ?? UniqueEmail(), password = Password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        return body!.UserId;
    }

    protected async Task<(Guid UserId, string Email)> RegisterAndConfirmAsync()
    {
        var email = UniqueEmail();
        var userId = await RegisterAsync(email);

        await UsingServicesAsync(async sp =>
        {
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
            (await userManager.ConfirmEmailAsync(user!, token)).Succeeded.Should().BeTrue();
        });

        return (userId, email);
    }

    protected Task<string> GetConfirmationCodeAsync(Guid userId) =>
        UsingServicesAsync(async sp =>
        {
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            return IdentityTokenEncoder.Encode(await userManager.GenerateEmailConfirmationTokenAsync(user!));
        });

    protected Task<string> GetPasswordResetCodeAsync(Guid userId) =>
        UsingServicesAsync(async sp =>
        {
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            return IdentityTokenEncoder.Encode(await userManager.GeneratePasswordResetTokenAsync(user!));
        });

    protected sealed record RegisterUserResponse(Guid UserId);
}
