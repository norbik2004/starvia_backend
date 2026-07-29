using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read.Models;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Read.Models;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Read;

internal sealed class UserPlatformReadConfiguration
    : IEntityTypeConfiguration<UserPlatformReadModel>
{
    public void Configure(EntityTypeBuilder<UserPlatformReadModel> builder)
    {
        builder.ToTable("UserPlatforms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AccountUsername);
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.AccountComment);
        builder.Property(x => x.ProfilePictureLink);
        builder.Property(x => x.ExternalAccountId);


        builder.HasOne(x => x.Platform)
            .WithMany()
            .HasForeignKey(x => x.PlatformId);
    }
}
