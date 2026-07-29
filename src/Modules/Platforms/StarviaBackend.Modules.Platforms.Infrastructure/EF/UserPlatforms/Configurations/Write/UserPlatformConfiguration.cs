using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Write;

internal sealed class UserPlatformConfiguration : IEntityTypeConfiguration<UserPlatform>
{
    public void Configure(EntityTypeBuilder<UserPlatform> builder)
    {
        builder.ToTable("UserPlatforms");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.AccessToken);
        builder.Property(u => u.AccountComment);
        builder.Property(u => u.AccountUsername);
        builder.Property(u => u.LastModifiedBy);
        builder.Property(u => u.ProfilePictureLink);
        builder.Property(u => u.AccountUsername);


        builder.HasOne(x => x.Platform)
            .WithMany(x => x.UserPlatforms)
            .HasForeignKey(x => x.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
