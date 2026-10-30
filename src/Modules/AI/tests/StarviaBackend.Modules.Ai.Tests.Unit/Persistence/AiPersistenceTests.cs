using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Read.Models;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Repositories;
using StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Read.Models;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Persistence;

/// <summary>
/// Verifies the EF mapping, the migrations and the change tracking of the chat aggregate without a
/// database: the contexts are only used to build the model, no connection is ever opened.
/// </summary>
public sealed class AiPersistenceTests
{
    private const string UnusedConnectionString = "Host=localhost;Database=unused;Username=unused;Password=unused";
    private static readonly DateTime Now = new(2026, 10, 29, 21, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void WriteModel_MapsChatsAndMessagesToTheAiSchema()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var chats = context.Model.FindEntityType(typeof(Chat))!;
        var messages = context.Model.FindEntityType(typeof(ChatMessage))!;

        // Assert
        chats.GetTableName().Should().Be("Chats");
        chats.GetSchema().Should().Be("ai");
        messages.GetTableName().Should().Be("ChatMessages");
        messages.GetSchema().Should().Be("ai");
    }

    [Fact]
    public void WriteModel_LeavesKeyGenerationToTheDomain()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var chatKey = context.Model.FindEntityType(typeof(Chat))!.FindPrimaryKey()!.Properties.Single();
        var messageKey = context.Model.FindEntityType(typeof(ChatMessage))!.FindPrimaryKey()!.Properties.Single();

        // Assert
        chatKey.ValueGenerated.Should().Be(ValueGenerated.Never);
        messageKey.ValueGenerated.Should().Be(ValueGenerated.Never);
    }

    [Fact]
    public void WriteModel_AppliesTheDomainLengthLimits()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var title = context.Model.FindEntityType(typeof(Chat))!.FindProperty(nameof(Chat.Title))!;
        var model = context.Model.FindEntityType(typeof(ChatMessage))!.FindProperty(nameof(ChatMessage.Model))!;
        var content = context.Model.FindEntityType(typeof(ChatMessage))!.FindProperty(nameof(ChatMessage.Content))!;

        // Assert
        title.GetMaxLength().Should().Be(Chat.MaxTitleLength);
        title.IsNullable.Should().BeFalse();
        model.GetMaxLength().Should().Be(ChatMessage.MaxModelLength);
        model.IsNullable.Should().BeTrue();
        content.GetMaxLength().Should().BeNull("prompts and answers are unbounded text");
        content.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void WriteModel_DeletesMessagesTogetherWithTheirChat()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var foreignKey = context.Model.FindEntityType(typeof(ChatMessage))!.GetForeignKeys().Single();

        // Assert
        foreignKey.PrincipalEntityType.ClrType.Should().Be(typeof(Chat));
        foreignKey.Properties.Single().Name.Should().Be(nameof(ChatMessage.ChatId));
        foreignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    [Fact]
    public void WriteModel_EnforcesOneMessagePerPositionInAChat()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var indexes = context.Model.FindEntityType(typeof(ChatMessage))!.GetIndexes();

        // Assert
        indexes.Should().ContainSingle(i =>
            i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "ChatId", "Sequence" }));
    }

    [Fact]
    public void WriteModel_IndexesTheChatHistoryPerUser()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var indexes = context.Model.FindEntityType(typeof(Chat))!.GetIndexes();

        // Assert
        indexes.Should().ContainSingle(i =>
            !i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "UserId", "LastMessageAt" }));
    }

    [Fact]
    public void Migrations_MatchTheCurrentModel()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var hasPendingChanges = context.Database.HasPendingModelChanges();

        // Assert
        hasPendingChanges.Should().BeFalse("a migration must be added whenever the AI model changes");
    }

    [Fact]
    public void Migrations_CreateTheTablesFromTheInitialMigration()
    {
        // Arrange
        using var context = CreateWriteContext();

        // Act
        var migrations = context.Database.GetMigrations().ToList();

        // Assert
        migrations.Should().ContainSingle().Which.Should().EndWith("_Initial");
    }

    [Theory]
    [InlineData(typeof(ChatReadModel), typeof(Chat))]
    [InlineData(typeof(ChatMessageReadModel), typeof(ChatMessage))]
    public void ReadModel_MapsToTheSameColumnsAsTheWriteModel(Type readModelType, Type entityType)
    {
        // Arrange
        using var readContext = CreateReadContext();
        using var writeContext = CreateWriteContext();

        var readEntity = readContext.Model.FindEntityType(readModelType)!;
        var writeEntity = writeContext.Model.FindEntityType(entityType)!;
        var readTable = StoreObjectIdentifier.Table(readEntity.GetTableName()!, readEntity.GetSchema());
        var writeTable = StoreObjectIdentifier.Table(writeEntity.GetTableName()!, writeEntity.GetSchema());

        // Assert
        readTable.Should().Be(writeTable);

        foreach (var readProperty in readEntity.GetProperties())
        {
            var columnName = readProperty.GetColumnName(readTable);
            var writeProperty = writeEntity.GetProperties()
                .SingleOrDefault(p => p.GetColumnName(writeTable) == columnName);

            writeProperty.Should().NotBeNull($"column '{columnName}' of the read model must exist in the table");
            readProperty.GetColumnType().Should().Be(writeProperty!.GetColumnType(), columnName);
            readProperty.IsNullable.Should().Be(writeProperty.IsNullable, columnName);
        }
    }

    [Fact]
    public async Task ReadContext_RejectsSaving()
    {
        // Arrange
        await using var context = CreateReadContext();

        // Act
        var act = () => context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void MessageAddedToATrackedChat_IsInsertedRatherThanUpdated()
    {
        // Arrange - a chat that was loaded from the database and is therefore tracked as unchanged
        using var context = CreateWriteContext();
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");
        context.Attach(chat);
        context.Entry(chat).State.Should().Be(EntityState.Unchanged);

        // Act
        var message = chat.AddUserMessage("Question", Now);
        context.ChangeTracker.DetectChanges();

        // Assert
        context.Entry(message).State.Should().Be(EntityState.Added);
        context.Entry(chat).State.Should().Be(EntityState.Modified, "LastMessageAt changed");
    }

    [Fact]
    public void DbSetUpdate_OnAnUntrackedChat_WouldMarkNewMessagesAsModified()
    {
        // Documents why ChatRepository.UpdateAsync refuses untracked chats instead of calling DbSet.Update:
        // the save would try to UPDATE message rows that were never inserted.
        // Arrange
        using var context = CreateWriteContext();
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");
        var message = chat.AddUserMessage("Question", Now);

        // Act
        context.Chats.Update(chat);

        // Assert
        context.Entry(message).State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task Repository_RefusesToUpdateAChatThatIsNotTracked()
    {
        // Arrange
        await using var context = CreateWriteContext();
        var repository = new ChatRepository(context);
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act
        var act = () => repository.UpdateAsync(chat);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static AiWriteDbContext CreateWriteContext() =>
        new(new DbContextOptionsBuilder<AiWriteDbContext>().UseNpgsql(UnusedConnectionString).Options);

    private static AiReadDbContext CreateReadContext() =>
        new(new DbContextOptionsBuilder<AiReadDbContext>().UseNpgsql(UnusedConnectionString).Options);
}
