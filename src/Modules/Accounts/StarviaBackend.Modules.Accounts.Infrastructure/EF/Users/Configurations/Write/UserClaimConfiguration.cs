using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Write;

internal sealed class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("UserClaims");
        builder.Property(c => c.ClaimType).HasMaxLength(256);
    }
}
