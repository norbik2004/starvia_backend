
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Write;

internal class ContentCalendarConfiguration : IEntityTypeConfiguration<ContentCalendar>
{
    public void Configure(EntityTypeBuilder<ContentCalendar> builder)
    {
        builder.ToTable("ContentCalendars");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).IsRequired();
    }
}
