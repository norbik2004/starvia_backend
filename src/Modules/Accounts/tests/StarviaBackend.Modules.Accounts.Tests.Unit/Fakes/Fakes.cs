using MassTransit;
using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Accounts.Tests.Unit.Fakes;

internal sealed class FakeClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; set; } = utcNow;
}

internal sealed class InMemoryUsersRepository : IUsersRepository
{
    public List<User> Users { get; } = [];

    public List<User> Updated { get; } = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => Same(u.Email, email)));

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(u => Same(u.Email, email)));

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        Updated.Add(user);
        return Task.CompletedTask;
    }

    private static bool Same(string? left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}

internal sealed class FakeAuthManager : IAuthManager
{
    public List<(string UserId, string Email, IReadOnlyList<string> Roles)> Issued { get; } = [];

    public JsonWebToken CreateToken(string userId, string email, IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        Issued.Add((userId, email, roleList));
        return new JsonWebToken($"token-for-{userId}", 1234, userId, email, roleList);
    }
}

/// <summary>Captures what a handler publishes; everything but the plain generic overload is unsupported.</summary>
internal sealed class RecordingPublishEndpoint : IPublishEndpoint
{
    public List<object> Published { get; } = [];

    public IEnumerable<T> PublishedOf<T>() => Published.OfType<T>();

    public Task Publish<T>(T message, CancellationToken cancellationToken = default)
        where T : class
    {
        Published.Add(message);
        return Task.CompletedTask;
    }

    public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default)
        where T : class => Publish(message, cancellationToken);

    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        where T : class => Publish(message, cancellationToken);

    public Task Publish(object message, CancellationToken cancellationToken = default)
    {
        Published.Add(message);
        return Task.CompletedTask;
    }

    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) =>
        Publish(message, cancellationToken);

    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) =>
        Publish(message, cancellationToken);

    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) =>
        Publish(message, cancellationToken);

    public Task Publish<T>(object values, CancellationToken cancellationToken = default)
        where T : class => throw new NotSupportedException();

    public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default)
        where T : class => throw new NotSupportedException();

    public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        where T : class => throw new NotSupportedException();

    public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotSupportedException();
}

/// <summary>
/// A <see cref="UserManager{TUser}"/> whose Identity behaviour is scripted by the test instead of
/// going through a store. Only the members the handlers use are overridden.
/// </summary>
internal sealed class FakeUserManager()
    : UserManager<User>(new NullUserStore(), null!, null!, null!, null!, null!, null!, null!, null!)
{
    public List<User> Created { get; } = [];

    public List<string> CreatedWithPassword { get; } = [];

    public List<(User User, string Role)> RoleAssignments { get; } = [];

    public List<(User User, string Token)> ConfirmAttempts { get; } = [];

    public List<(User User, string Token, string Password)> ResetAttempts { get; } = [];

    /// <summary>Plain-text passwords by user id, consulted by <see cref="CheckPasswordAsync"/>.</summary>
    public Dictionary<Guid, string> Passwords { get; } = [];

    public Dictionary<Guid, List<string>> Roles { get; } = [];

    public List<User> KnownUsers { get; } = [];

    public IdentityResult CreateResult { get; set; } = IdentityResult.Success;

    public IdentityResult ConfirmResult { get; set; } = IdentityResult.Success;

    public IdentityResult ResetResult { get; set; } = IdentityResult.Success;

    public string EmailConfirmationToken { get; set; } = "email-confirmation-token";

    public string PasswordResetToken { get; set; } = "password-reset-token";

    public override Task<IdentityResult> CreateAsync(User user, string password)
    {
        if (CreateResult.Succeeded)
        {
            Created.Add(user);
            CreatedWithPassword.Add(password);
            KnownUsers.Add(user);
        }

        return Task.FromResult(CreateResult);
    }

    public override Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        RoleAssignments.Add((user, role));
        return Task.FromResult(IdentityResult.Success);
    }

    public override Task<User?> FindByEmailAsync(string email) =>
        Task.FromResult(KnownUsers.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public override Task<bool> CheckPasswordAsync(User user, string password) =>
        Task.FromResult(Passwords.TryGetValue(user.Id, out var expected) && expected == password);

    public override Task<IList<string>> GetRolesAsync(User user) =>
        Task.FromResult<IList<string>>(Roles.TryGetValue(user.Id, out var roles) ? [.. roles] : []);

    public override Task<string> GenerateEmailConfirmationTokenAsync(User user) =>
        Task.FromResult(EmailConfirmationToken);

    public override Task<string> GeneratePasswordResetTokenAsync(User user) =>
        Task.FromResult(PasswordResetToken);

    public override Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        ConfirmAttempts.Add((user, token));

        if (ConfirmResult.Succeeded)
        {
            user.ConfirmEmail();
        }

        return Task.FromResult(ConfirmResult);
    }

    public override Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        ResetAttempts.Add((user, token, newPassword));
        return Task.FromResult(ResetResult);
    }

    private sealed class NullUserStore : IUserStore<User>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
