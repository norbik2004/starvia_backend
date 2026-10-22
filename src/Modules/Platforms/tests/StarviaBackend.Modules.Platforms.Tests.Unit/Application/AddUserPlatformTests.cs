using FluentAssertions;
using FluentValidation.TestHelper;
using StarviaBackend.Modules.Platforms.Application.Platforms.Queries.BrowsePlatforms;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Commands.AddUserPlatform;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Application;

public sealed class AddUserPlatformValidatorTests
{
    private readonly AddUserPlatformValidator _validator = new();

    private static AddUserPlatformCommand Command(
        string userName = "janedoe",
        string? comment = "Work account",
        Guid? platformId = null) =>
        new(new AddUserPlatformRequest(platformId ?? Guid.NewGuid(), userName, comment), Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Missing_comment_passes()
    {
        _validator.TestValidate(Command(comment: null)).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Empty_or_too_short_username_fails(string userName)
    {
        _validator.TestValidate(Command(userName: userName)).ShouldHaveValidationErrorFor(x => x.request.AccountUserName);
    }

    [Fact]
    public void Username_of_25_letters_passes_and_26_fails()
    {
        _validator.TestValidate(Command(userName: new string('a', 25))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(userName: new string('a', 26)))
            .ShouldHaveValidationErrorFor(x => x.request.AccountUserName);
    }

    [Theory]
    [InlineData("jane123")]
    [InlineData("jane doe")]
    [InlineData("jane_doe")]
    [InlineData("jane.doe")]
    [InlineData("jane-doe")]
    public void Username_with_anything_but_letters_fails(string userName)
    {
        _validator.TestValidate(Command(userName: userName)).ShouldHaveValidationErrorFor(x => x.request.AccountUserName);
    }

    [Fact]
    public void Comment_of_250_characters_passes_and_251_fails()
    {
        _validator.TestValidate(Command(comment: new string('c', 250))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(comment: new string('c', 251)))
            .ShouldHaveValidationErrorFor(x => x.request.AccountComment);
    }

    [Fact]
    public void Empty_platform_id_fails()
    {
        _validator.TestValidate(Command(platformId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.request.PlatformId);
    }
}

public sealed class AddUserPlatformHandlerTests
{
    private readonly InMemoryPlatformRepository _platforms = new();
    private readonly InMemoryUserPlatformRepository _userPlatforms = new();

    private AddUserPlatformHandler CreateHandler() => new(_userPlatforms, _platforms);

    [Fact]
    public async Task HandleAsync_WhenPlatformDoesNotExist_Throws()
    {
        // Arrange
        var command = new AddUserPlatformCommand(new AddUserPlatformRequest(Guid.NewGuid(), "janedoe", null), Guid.NewGuid());

        // Act
        var act = () => CreateHandler().HandleAsync(command);

        // Assert
        var exception = await act.Should().ThrowAsync<PlatformNotFoundException>();
        exception.Which.Code.Should().Be("platform_not_found");
        _userPlatforms.UserPlatforms.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_StoresTheUsersPlatformAccount()
    {
        // Arrange
        var platform = _platforms.Seed();
        var userId = Guid.NewGuid();
        var command = new AddUserPlatformCommand(new AddUserPlatformRequest(platform.Id, "janedoe", "Work account"), userId);

        // Act
        var result = await CreateHandler().HandleAsync(command);

        // Assert
        var stored = _userPlatforms.UserPlatforms.Should().ContainSingle().Subject;
        result.UserPlatformId.Should().Be(stored.Id);
        stored.UserId.Should().Be(userId);
        stored.PlatformId.Should().Be(platform.Id);
        stored.AccountUsername.Should().Be("janedoe");
        stored.AccountComment.Should().Be("Work account");
    }

    [Fact]
    public async Task HandleAsync_AllowsTheSameUserToConnectSeveralAccounts()
    {
        // Arrange
        var platform = _platforms.Seed();
        var userId = Guid.NewGuid();

        // Act
        var first = await CreateHandler().HandleAsync(
            new AddUserPlatformCommand(new AddUserPlatformRequest(platform.Id, "firstaccount", null), userId));
        var second = await CreateHandler().HandleAsync(
            new AddUserPlatformCommand(new AddUserPlatformRequest(platform.Id, "secondaccount", null), userId));

        // Assert
        first.UserPlatformId.Should().NotBe(second.UserPlatformId);
        _userPlatforms.UserPlatforms.Should().HaveCount(2);
    }
}

public sealed class BrowseQueriesTests
{
    [Theory]
    [InlineData(1, 20, 1, 20)]
    [InlineData(3, 50, 3, 50)]
    [InlineData(0, 20, 1, 20)]
    [InlineData(-5, 20, 1, 20)]
    [InlineData(1, 0, 1, 20)]
    [InlineData(1, 101, 1, 20)]
    [InlineData(1, 100, 1, 100)]
    public void BrowsePlatformsQuery_NormalizesPaging(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        // Act
        var query = new BrowsePlatformsQuery(page, pageSize);

        // Assert
        query.Page.Should().Be(expectedPage);
        query.PageSize.Should().Be(expectedPageSize);
    }

    [Theory]
    [InlineData(1, 20, 1, 20)]
    [InlineData(0, 20, 1, 20)]
    [InlineData(2, 0, 2, 20)]
    [InlineData(2, 500, 2, 20)]
    [InlineData(2, 100, 2, 100)]
    public void BrowseUserPlatformsQuery_NormalizesPaging(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        // Act
        var query = new BrowseUserPlatformsQuery(page, pageSize);

        // Assert
        query.Page.Should().Be(expectedPage);
        query.PageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public void Defaults_AreFirstPageOfTwenty()
    {
        new BrowsePlatformsQuery().Should().Be(new BrowsePlatformsQuery(1, 20));
        new BrowseUserPlatformsQuery().Should().Be(new BrowseUserPlatformsQuery(1, 20));
    }
}
