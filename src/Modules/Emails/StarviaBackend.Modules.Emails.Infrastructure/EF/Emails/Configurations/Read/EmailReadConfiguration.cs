using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Read.Models;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Read;

internal sealed class EmailReadConfiguration : IEntityTypeConfiguration<EmailReadModel>
{
    public void Configure(EntityTypeBuilder<EmailReadModel> builder)
    {
        builder.ToTable("Emails");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Body);
        builder.Property(x => x.SentAt);
        builder.Property(x => x.SentTo);
        builder.Property(x => x.Title);
    }
}
