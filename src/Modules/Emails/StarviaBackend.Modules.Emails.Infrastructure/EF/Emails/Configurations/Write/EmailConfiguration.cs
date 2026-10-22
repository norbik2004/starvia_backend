using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Write;

internal sealed class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("Emails");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.SentAt).IsRequired();
        builder.Property(u => u.Body).IsRequired();
        builder.Property(u => u.Title).IsRequired();
        builder.Property(u => u.SentTo).IsRequired();
    }
}
