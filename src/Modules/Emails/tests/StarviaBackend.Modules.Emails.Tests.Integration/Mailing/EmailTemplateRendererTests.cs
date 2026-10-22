using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Application.Emails.Links;
using StarviaBackend.Modules.Emails.Application.Emails.Messages;
using StarviaBackend.Modules.Emails.Application.Emails.Tokens;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Infrastructure.Mailing;
using StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;
using StarviaBackend.Modules.Emails.Tests.Integration.Support;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Mailing;

/// <summary>
/// Renders the real Razor templates shipped with the module (layout, logo and per-type bodies)
/// through the real <see cref="EmailTemplateRenderer"/>; only the account-token source is stubbed.
/// </summary>
public sealed class EmailTemplateRendererTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    // RazorLight compiles each template on first use, so share one engine across the tests.
    private static readonly RazorEmailRenderer Razor = new();

    private readonly StubAccountTokenProvider _tokens = new();
    private readonly EmailTemplateRenderer _renderer;

    public EmailTemplateRendererTests()
    {
        var urls = new StubAppUrls("https://app.example.com/");
        var services = new ServiceCollection();
        services.AddScoped<IAccountTokenProvider>(_ => _tokens);
        services.AddScoped<IAppUrls>(_ => urls);
        services.AddScoped<ConfirmAccountEmail>();
        services.AddScoped<ResetPasswordEmail>();
        services.AddScoped<WelcomingEmail>();

        _renderer = new EmailTemplateRenderer(Razor, services.BuildServiceProvider());
    }

    [Fact]
    public async Task Welcoming_email_greets_the_user_and_links_to_login()
    {
        var template = await _renderer.RenderAsync(EmailType.WelcomingEmail, UserId, "user@example.com");

        template.Subject.Should().Be(WelcomingEmail.Subject);
        var html = WebUtility.HtmlDecode(template.HtmlBody);
        html.Should().Contain("user@example.com");
        html.Should().Contain("https://app.example.com/login");
    }

    [Fact]
    public async Task Confirm_account_email_carries_the_confirmation_link()
    {
        _tokens.ConfirmationToken = "confirm+me/please";

        var template = await _renderer.RenderAsync(EmailType.ConfirmAccountEmail, UserId, "user@example.com");

        template.Subject.Should().Be(ConfirmAccountEmail.Subject);
        var html = WebUtility.HtmlDecode(template.HtmlBody);
        html.Should().Contain("user@example.com");
        html.Should().Contain(AccountEmailLink.ConfirmAccount("https://app.example.com", UserId, "confirm+me/please"));
    }

    [Fact]
    public async Task Reset_password_email_carries_the_reset_link()
    {
        _tokens.PasswordResetToken = "reset+me/please";

        var template = await _renderer.RenderAsync(EmailType.ResetPasswordEmail, UserId, "user@example.com");

        template.Subject.Should().Be(ResetPasswordEmail.Subject);
        var html = WebUtility.HtmlDecode(template.HtmlBody);
        html.Should().Contain("user@example.com");
        html.Should().Contain(AccountEmailLink.ResetPassword("https://app.example.com", UserId, "reset+me/please"));
    }

    [Theory]
    [InlineData(EmailType.WelcomingEmail)]
    [InlineData(EmailType.ConfirmAccountEmail)]
    [InlineData(EmailType.ResetPasswordEmail)]
    public async Task Every_type_is_wrapped_in_the_shared_layout_with_the_subject_as_title_and_the_inline_logo(EmailType type)
    {
        var template = await _renderer.RenderAsync(type, UserId, "user@example.com");

        template.HtmlBody.Should().Contain("<!DOCTYPE html>");
        // Razor encodes non-ASCII characters (the Polish subjects) as numeric entities, so compare decoded.
        WebUtility.HtmlDecode(template.HtmlBody).Should().Contain($"<title>{template.Subject}</title>");
        template.HtmlBody.Should().Contain("src=\"data:image/png;base64,");
    }

    [Fact]
    public async Task The_body_is_embedded_as_markup_not_escaped_text()
    {
        var template = await _renderer.RenderAsync(EmailType.WelcomingEmail, UserId, "user@example.com");

        template.HtmlBody.Should().NotContain("&lt;p");
        template.HtmlBody.Should().NotContain("&lt;table");
    }

    [Fact]
    public async Task User_supplied_text_is_html_encoded()
    {
        var template = await _renderer.RenderAsync(EmailType.WelcomingEmail, UserId, "<script>alert(1)</script>@example.com");

        template.HtmlBody.Should().NotContain("<script>alert(1)</script>");
        template.HtmlBody.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public async Task A_rejected_account_token_surfaces_instead_of_rendering_a_broken_email()
    {
        _tokens.Failure = new AccountTokenRejectedException(UserId, "email_already_confirmed", "already done");

        var act = () => _renderer.RenderAsync(EmailType.ConfirmAccountEmail, UserId, "user@example.com");

        await act.Should().ThrowAsync<AccountTokenRejectedException>();
    }

    [Theory]
    [InlineData(EmailType.WelcomingEmail, typeof(WelcomingEmail))]
    [InlineData(EmailType.ConfirmAccountEmail, typeof(ConfirmAccountEmail))]
    [InlineData(EmailType.ResetPasswordEmail, typeof(ResetPasswordEmail))]
    public async Task Only_the_message_for_the_requested_type_is_resolved(EmailType type, Type expected)
    {
        var urls = new StubAppUrls("https://app.example.com/");
        var resolved = new List<Type>();
        var services = new ServiceCollection();
        services.AddScoped<IAccountTokenProvider>(_ => _tokens);
        services.AddScoped<IAppUrls>(_ => urls);
        services.AddScoped(_ => { resolved.Add(typeof(ConfirmAccountEmail)); return new ConfirmAccountEmail(_tokens, urls); });
        services.AddScoped(_ => { resolved.Add(typeof(ResetPasswordEmail)); return new ResetPasswordEmail(_tokens, urls); });
        services.AddScoped(_ => { resolved.Add(typeof(WelcomingEmail)); return new WelcomingEmail(urls); });
        var renderer = new EmailTemplateRenderer(Razor, services.BuildServiceProvider());

        await renderer.RenderAsync(type, UserId, "user@example.com");

        resolved.Should().Equal(expected);
    }

    [Fact]
    public async Task An_unsupported_email_type_is_rejected()
    {
        var act = () => _renderer.RenderAsync((EmailType)999, UserId, "user@example.com");

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
