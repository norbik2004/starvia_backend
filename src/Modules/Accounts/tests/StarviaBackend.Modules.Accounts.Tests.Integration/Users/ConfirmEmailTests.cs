using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.App;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class ConfirmEmailTests(AccountsApp app) : AccountsIntegrationTest(app)
{
    [Fact]
    public async Task Confirm_email_with_valid_code_sets_confirmed_and_redirects_to_frontend()
    {
        var email = UniqueEmail();
        var userId = await Register(email);
        var link = await BuildConfirmationLink(userId);

        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync(ToRelative(link));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        AssertFrontendRedirect(response.Headers.Location, expectedStatus: "success");

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
        var link = await BuildConfirmationLink(userId);

        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        (await client.GetAsync(ToRelative(link))).StatusCode.Should().Be(HttpStatusCode.Redirect);

        var second = await client.GetAsync(ToRelative(link));
        second.StatusCode.Should().Be(HttpStatusCode.Redirect);
        AssertFrontendRedirect(second.Headers.Location, expectedStatus: "success");
    }

    [Fact]
    public async Task Confirm_email_with_invalid_code_redirects_with_error()
    {
        var email = UniqueEmail();
        var userId = await Register(email);

        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync($"/v1/accounts/confirm-email?userId={userId}&code=not-a-valid-code");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        AssertFrontendRedirect(response.Headers.Location, expectedStatus: "error");
        response.Headers.Location!.Query.Should().Contain("error=");

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

    private async Task<string> BuildConfirmationLink(Guid userId)
    {
        return await UsingServicesAsync(async sp =>
        {
            var userManager = sp.GetRequiredService<UserManager<User>>();
            var appUrls = sp.GetRequiredService<IAppUrls>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
            return EmailConfirmationLink.Build(appUrls.ApiBaseUrl, userId, token);
        });
    }

    private static void AssertFrontendRedirect(Uri? location, string expectedStatus)
    {
        location.Should().NotBeNull();
        location!.GetLeftPart(UriPartial.Path).Should().Be("http://localhost:4200/email-confirmed");
        location.Query.Should().Contain($"status={expectedStatus}");

        var ticket = GetQueryParam(location, "ticket");
        ticket.Should().NotBeNullOrWhiteSpace();
        ticket!.Length.Should().BeGreaterThanOrEqualTo(EmailConfirmationLink.TicketLength);
    }

    private static string? GetQueryParam(Uri location, string name)
    {
        foreach (var part in location.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            if (separator < 0)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(part[..separator]);
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(part[(separator + 1)..]);
            }
        }

        return null;
    }

    private static string ToRelative(string absoluteUrl) => new Uri(absoluteUrl).PathAndQuery;

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private sealed record RegisterUserResponse(Guid UserId);
}
