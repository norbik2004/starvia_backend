using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Queries;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Repositories;
using StarviaBackend.Shared.Abstractions.Email;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Persistence;

[Collection(EmailsDatabase.CollectionName)]
public sealed class EmailRepositoryTests(EmailsDatabase database) : IAsyncLifetime
{
    public Task InitializeAsync() => database.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Added_email_is_persisted_with_all_its_values()
    {
        var sentAt = new DateTime(2026, 10, 20, 12, 30, 15, DateTimeKind.Utc);
        var email = Email.Create("Subject", "<p>Body</p>", sentAt, "user@example.com", EmailType.ResetPasswordEmail);

        await using (var writeContext = database.CreateWriteContext())
        {
            await new EmailRepository(writeContext).AddAsync(email);
        }

        await using var verifyContext = database.CreateWriteContext();
        var stored = await verifyContext.Emails.AsNoTracking().SingleAsync();
        stored.Id.Should().Be(email.Id);
        stored.Title.Should().Be("Subject");
        stored.Body.Should().Be("<p>Body</p>");
        stored.SentTo.Should().Be("user@example.com");
        stored.Type.Should().Be(EmailType.ResetPasswordEmail);
        stored.SentAt.Should().Be(sentAt);
    }

    [Fact]
    public async Task Adding_saves_immediately_without_a_separate_commit()
    {
        await using var writeContext = database.CreateWriteContext();

        await new EmailRepository(writeContext).AddAsync(
            Email.Create("t", "b", DateTime.UtcNow, "a@example.com", EmailType.WelcomingEmail));

        await using var otherContext = database.CreateWriteContext();
        (await otherContext.Emails.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Several_emails_to_the_same_address_are_all_kept()
    {
        await using var writeContext = database.CreateWriteContext();
        var repository = new EmailRepository(writeContext);

        await repository.AddAsync(Email.Create("one", "b", DateTime.UtcNow, "a@example.com", EmailType.WelcomingEmail));
        await repository.AddAsync(Email.Create("two", "b", DateTime.UtcNow, "a@example.com", EmailType.WelcomingEmail));

        await using var otherContext = database.CreateWriteContext();
        (await otherContext.Emails.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task The_read_context_cannot_be_used_to_save_changes()
    {
        await using var readContext = database.CreateReadContext();

        var act = () => readContext.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

[Collection(EmailsDatabase.CollectionName)]
public sealed class BrowseEmailsHandlerTests(EmailsDatabase database) : IAsyncLifetime
{
    private static readonly DateTime Start = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

    public Task InitializeAsync() => database.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedAsync(params (string Title, int DayOffset)[] emails)
    {
        await using var writeContext = database.CreateWriteContext();
        var repository = new EmailRepository(writeContext);

        foreach (var (title, dayOffset) in emails)
        {
            await repository.AddAsync(Email.Create(
                title, $"<p>{title}</p>", Start.AddDays(dayOffset), $"{title}@example.com", EmailType.WelcomingEmail));
        }
    }

    private async Task<PagedResult<EmailDto>> BrowseAsync(BrowseEmailsQuery query)
    {
        await using var readContext = database.CreateReadContext();
        return await new BrowseEmailsHandler(readContext).HandleAsync(query);
    }

    [Fact]
    public async Task No_emails_gives_an_empty_page_that_echoes_the_requested_paging()
    {
        var result = await BrowseAsync(new BrowseEmailsQuery(3, 7));

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(3);
        result.PageSize.Should().Be(7);
    }

    [Fact]
    public async Task Emails_are_mapped_to_dtos()
    {
        await SeedAsync(("alpha", 0));

        var result = await BrowseAsync(new BrowseEmailsQuery());

        var dto = result.Items.Should().ContainSingle().Subject;
        dto.Id.Should().NotBeEmpty();
        dto.Title.Should().Be("alpha");
        dto.SentTo.Should().Be("alpha@example.com");
        dto.Body.Should().Be("<p>alpha</p>");
        dto.SentAt.Should().Be(Start);
    }

    [Fact]
    public async Task Emails_come_back_oldest_first_regardless_of_insertion_order()
    {
        await SeedAsync(("third", 2), ("first", 0), ("second", 1));

        var result = await BrowseAsync(new BrowseEmailsQuery());

        result.Items.Select(x => x.Title).Should().Equal("first", "second", "third");
    }

    [Fact]
    public async Task Paging_slices_the_ordered_emails_and_reports_the_totals()
    {
        await SeedAsync(("e1", 0), ("e2", 1), ("e3", 2), ("e4", 3), ("e5", 4));

        var first = await BrowseAsync(new BrowseEmailsQuery(1, 2));
        var second = await BrowseAsync(new BrowseEmailsQuery(2, 2));
        var last = await BrowseAsync(new BrowseEmailsQuery(3, 2));

        first.Items.Select(x => x.Title).Should().Equal("e1", "e2");
        second.Items.Select(x => x.Title).Should().Equal("e3", "e4");
        last.Items.Select(x => x.Title).Should().Equal("e5");

        foreach (var page in new[] { first, second, last })
        {
            page.TotalCount.Should().Be(5);
            page.TotalPages.Should().Be(3);
            page.PageSize.Should().Be(2);
        }

        (first.Page, second.Page, last.Page).Should().Be((1, 2, 3));
    }

    [Fact]
    public async Task A_page_past_the_end_has_no_items_but_still_reports_the_total()
    {
        await SeedAsync(("e1", 0), ("e2", 1));

        var result = await BrowseAsync(new BrowseEmailsQuery(5, 10));

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(5);
    }
}
