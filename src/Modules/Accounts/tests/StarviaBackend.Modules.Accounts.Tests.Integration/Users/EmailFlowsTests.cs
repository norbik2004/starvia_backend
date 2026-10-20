using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Email;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

/// <summary>
/// What the account flows hand to the Emails module: the <see cref="SendEmailRequestedEvent"/>s
/// that actually reach the (in-memory) bus when the endpoints are called over HTTP.
/// </summary>
public sealed class AccountEmailRequestsTests : AccountsFlowTest, IDisposable
{
    private readonly EmailRequestRecorder _recorder;

    public AccountEmailRequestsTests(AccountsApp app)
        : base(app)
    {
        _recorder = new EmailRequestRecorder(app.Services.GetRequiredService<IBus>());
    }

    public void Dispose() => _recorder.Dispose();

    private Task<bool> WasRequestedAsync(Guid userId, EmailType type) =>
        _recorder.WaitForAsync(m => m.UserId == userId && m.EmailType == type);

    private int RequestedCount(Guid userId, EmailType type) =>
        _recorder.Messages.Count(m => m.UserId == userId && m.EmailType == type);

    [Fact]
    public async Task Registering_queues_a_confirm_account_email_for_the_new_user()
    {
        var email = UniqueEmail();
        var userId = await RegisterAsync(email);

        (await WasRequestedAsync(userId, EmailType.ConfirmAccountEmail)).Should().BeTrue();
        _recorder.Messages.Where(m => m.UserId == userId).Should().ContainSingle()
            .Which.Email.Should().Be(email);
    }

    [Fact]
    public async Task Forgot_password_for_a_known_email_queues_a_reset_password_email()
    {
        var (userId, email) = await RegisterAndConfirmAsync();

        (await Client.PostAsJsonAsync("/v1/accounts/forgot-password", new { email })).EnsureSuccessStatusCode();

        (await WasRequestedAsync(userId, EmailType.ResetPasswordEmail)).Should().BeTrue();
    }

    [Fact]
    public async Task Forgot_password_for_an_unknown_email_queues_nothing()
    {
        var email = UniqueEmail();

        var response = await Client.PostAsJsonAsync("/v1/accounts/forgot-password", new { email });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _recorder.Messages.Where(m => m.Email == email).Should().BeEmpty();
    }

    [Fact]
    public async Task Forgot_password_with_a_malformed_email_is_rejected_by_validation()
    {
        var response = await Client.PostAsJsonAsync("/v1/accounts/forgot-password", new { email = "not-an-email" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Confirming_the_email_queues_the_welcoming_email_exactly_once()
    {
        var userId = await RegisterAsync();
        var code = await GetConfirmationCodeAsync(userId);

        (await Client.PostAsJsonAsync("/v1/accounts/confirm-email", new { userId, code })).EnsureSuccessStatusCode();
        (await Client.PostAsJsonAsync("/v1/accounts/confirm-email", new { userId, code })).EnsureSuccessStatusCode();

        (await WasRequestedAsync(userId, EmailType.WelcomingEmail)).Should().BeTrue();
        RequestedCount(userId, EmailType.WelcomingEmail).Should().Be(1);
    }

    [Fact]
    public async Task A_rejected_confirmation_code_queues_no_welcoming_email()
    {
        var userId = await RegisterAsync();

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/confirm-email",
            new { userId, code = "not-a-valid-code" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        RequestedCount(userId, EmailType.WelcomingEmail).Should().Be(0);
    }
}

public sealed class ConfirmEmailEdgeCaseTests(AccountsApp app) : AccountsFlowTest(app)
{
    private const string Route = "/v1/accounts/confirm-email";

    [Fact]
    public async Task An_unknown_user_is_a_user_not_found_error()
    {
        var response = await Client.PostAsJsonAsync(Route, new { userId = Guid.NewGuid(), code = "abc" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "user_not_found");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task A_blank_code_is_rejected_by_validation(string code)
    {
        var userId = await RegisterAsync();

        var response = await Client.PostAsJsonAsync(Route, new { userId, code });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_code_that_is_not_base64_is_an_invalid_code_error()
    {
        var userId = await RegisterAsync();

        var response = await Client.PostAsJsonAsync(Route, new { userId, code = "!!!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "invalid_email_confirmation_code");
    }

    [Fact]
    public async Task A_code_issued_for_another_user_does_not_confirm_this_one()
    {
        var victim = await RegisterAsync();
        var other = await RegisterAsync();
        var otherCode = await GetConfirmationCodeAsync(other);

        var response = await Client.PostAsJsonAsync(Route, new { userId = victim, code = otherCode });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await WithDbAsync<AccountsWriteDbContext>(async db =>
            (await db.Users.AsNoTracking().SingleAsync(u => u.Id == victim)).EmailConfirmed.Should().BeFalse());
    }
}

public sealed class ResetPasswordEdgeCaseTests(AccountsApp app) : AccountsFlowTest(app)
{
    private const string Route = "/v1/accounts/reset-password";

    [Fact]
    public async Task An_unknown_user_is_a_user_not_found_error()
    {
        var response = await Client.PostAsJsonAsync(
            Route,
            new { userId = Guid.NewGuid(), code = "abc", password = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "user_not_found");
    }

    [Fact]
    public async Task A_too_short_new_password_is_rejected_and_the_old_one_keeps_working()
    {
        var (userId, email) = await RegisterAndConfirmAsync();
        var code = await GetPasswordResetCodeAsync(userId);

        var response = await Client.PostAsJsonAsync(Route, new { userId, code, password = "123" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync("/v1/accounts/sign-in", new { email, password = Password }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_reset_code_works_only_once()
    {
        var (userId, _) = await RegisterAndConfirmAsync();
        var code = await GetPasswordResetCodeAsync(userId);

        (await Client.PostAsJsonAsync(Route, new { userId, code, password = "N3wPassw0rd!" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var replay = await Client.PostAsJsonAsync(Route, new { userId, code, password = "An0therPassw0rd!" });

        replay.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await replay.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "invalid_password_reset_code");
    }

    [Fact]
    public async Task A_code_issued_for_another_user_cannot_reset_this_password()
    {
        var (victim, victimEmail) = await RegisterAndConfirmAsync();
        var (other, _) = await RegisterAndConfirmAsync();
        var otherCode = await GetPasswordResetCodeAsync(other);

        var response = await Client.PostAsJsonAsync(Route, new { userId = victim, code = otherCode, password = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync("/v1/accounts/sign-in", new { email = victimEmail, password = Password }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_code_that_is_not_base64_is_an_invalid_code_error()
    {
        var (userId, _) = await RegisterAndConfirmAsync();

        var response = await Client.PostAsJsonAsync(Route, new { userId, code = "!!!", password = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "invalid_password_reset_code");
    }
}
