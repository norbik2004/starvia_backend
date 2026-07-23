using StarviaBackend.Modules.Accounts.Core.Users.Entities;

namespace StarviaBackend.Modules.Accounts.Core.Users.Repositories;

internal interface IUsersRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
