using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Write;

internal class CalendarEntryConfiguration : IEntityTypeConfiguration<CalendarEntry>
{
    public void Configure(EntityTypeBuilder<CalendarEntry> builder)
    {
        builder.ToTable("CalendarEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ContentCalendarId).IsRequired();
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.ScheduledAt).IsRequired();
        builder.Property(x => x.Status).IsRequired();
    }
}
