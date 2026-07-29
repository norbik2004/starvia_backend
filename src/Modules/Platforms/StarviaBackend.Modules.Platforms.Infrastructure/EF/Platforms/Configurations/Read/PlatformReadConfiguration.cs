using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read.Models;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read;

internal sealed class PlatformReadConfiguration : IEntityTypeConfiguration<PlatformReadModel>
{
    public void Configure(EntityTypeBuilder<PlatformReadModel> builder)
    {
        builder.ToTable("Platforms");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.IsOnline);
        builder.Property(x => x.PlatformType);
    }
}
