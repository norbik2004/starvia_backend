using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Repositories;

internal sealed class UsersRepository(AccountsWriteDbContext dbContext) : IUsersRepository
{
    private readonly DbSet<User> _users = dbContext.Users;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToUpperInvariant();
        return _users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToUpperInvariant();
        return _users.AnyAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
