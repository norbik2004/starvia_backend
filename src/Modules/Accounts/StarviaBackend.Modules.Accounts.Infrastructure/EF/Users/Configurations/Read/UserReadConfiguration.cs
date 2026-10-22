using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Read.Models;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Read;

internal sealed class UserReadConfiguration : IEntityTypeConfiguration<UserReadModel>
{
    public void Configure(EntityTypeBuilder<UserReadModel> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email);
        builder.Property(u => u.UserName);
        builder.Property(u => u.CreatedAt);
        builder.Property(u => u.LastLoginAt);
    }
}
