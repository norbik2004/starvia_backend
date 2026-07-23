using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Write;

internal sealed class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder) => builder.ToTable("UserLogins");
}
