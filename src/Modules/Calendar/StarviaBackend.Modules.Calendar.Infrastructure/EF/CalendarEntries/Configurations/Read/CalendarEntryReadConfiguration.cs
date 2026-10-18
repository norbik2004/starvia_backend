using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Read.Models;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Read;

internal sealed class CalendarEntryReadConfiguration : IEntityTypeConfiguration<CalendarEntryReadModel>
{
    public void Configure(EntityTypeBuilder<CalendarEntryReadModel> builder)
    {
        builder.ToTable("CalendarEntries");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).IsRequired();
        builder.Property(u => u.CalendarId).IsRequired();
        builder.Property(u => u.PostId).IsRequired();
        builder.Property(u => u.ScheduledAt).IsRequired();
        builder.Property(u => u.Status).IsRequired();
    }
}
