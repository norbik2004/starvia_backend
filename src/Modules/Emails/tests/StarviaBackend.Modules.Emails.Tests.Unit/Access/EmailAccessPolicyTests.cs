using FluentAssertions;
using StarviaBackend.Modules.Emails.Core.Emails.Access;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Emails.Tests.Unit.Access;

public sealed class EmailAccessPolicyTests
{
    private static EmailAction[] AllActions => Enum.GetValues<EmailAction>();

    [Fact]
    public void ScopeFor_AnAdmin_CoversEverythingForEveryAction()
    {
        foreach (var action in AllActions)
        {
            EmailAccessPolicy.ScopeFor(action, new AccessActor(Guid.NewGuid(), IsAdmin: true))
                .Unrestricted.Should().BeTrue($"admins may {action}");
        }
    }

    [Fact]
    public void ScopeFor_APlainUserOrAnonymousCaller_CoversNothingForEveryAction()
    {
        foreach (var action in AllActions)
        {
            EmailAccessPolicy.ScopeFor(action, new AccessActor(Guid.NewGuid(), IsAdmin: false))
                .IsEmpty.Should().BeTrue($"plain users may not {action}");
            EmailAccessPolicy.ScopeFor(action, AccessActor.Anonymous)
                .IsEmpty.Should().BeTrue($"anonymous callers may not {action}");
        }
    }
}

public sealed class EmailAccessServiceTests
{
    [Fact]
    public void RequireScopeFor_ForAnAdmin_ReturnsAnUnrestrictedScope()
    {
        TestAccess.Admin().RequireScopeFor(EmailAction.BrowseEmails).Unrestricted.Should().BeTrue();
    }

    [Fact]
    public void RequireScopeFor_ForAPlainUserOrAnonymous_ThrowsAccessDenied()
    {
        var asUser = () => TestAccess.For(Guid.NewGuid()).RequireScopeFor(EmailAction.BrowseEmails);
        var asAnonymous = () => TestAccess.Anonymous().RequireScopeFor(EmailAction.BrowseEmails);

        asUser.Should().Throw<EmailAccessDeniedException>().Which.Code.Should().Be("email_access_denied");
        asAnonymous.Should().Throw<EmailAccessDeniedException>();
    }
}