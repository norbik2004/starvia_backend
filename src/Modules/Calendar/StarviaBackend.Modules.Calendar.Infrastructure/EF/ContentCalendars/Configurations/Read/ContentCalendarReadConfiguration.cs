using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Read.Models;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Read;

internal sealed class ContentCalendarReadConfiguration : IEntityTypeConfiguration<ContentCalendarReadModel>
{
    public void Configure(EntityTypeBuilder<ContentCalendarReadModel> builder)
    {
        builder.ToTable("ContentCalendars");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).IsRequired();
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.CreatedBy).IsRequired();

        builder.HasMany(u => u.CalendarEntries)
            .WithOne()
            .HasForeignKey(e => e.ContentCalendarId);
    }
}
