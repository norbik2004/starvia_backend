using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Write;

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder) => builder.ToTable("UserTokens");
}
