using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Write;

internal sealed class PlatformConfiguration : IEntityTypeConfiguration<Platform>
{
    public void Configure(EntityTypeBuilder<Platform> builder)
    {
        builder.ToTable("Platforms");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.PlatformType).IsRequired();
        builder.Property(u => u.IsOnline).IsRequired();

        builder.HasMany(x => x.UserPlatforms)
            .WithOne(x => x.Platform)
            .HasForeignKey(x => x.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
