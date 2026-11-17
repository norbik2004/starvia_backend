using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Abstractions.Platforms;
using StarviaBackend.Shared.Infrastructure.Serialization;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Platforms.Tests.Integration;

/// <summary>
/// All Platforms integration tests share one container (per collection) and run sequentially: the
/// Testcontainers connection string is passed via a process-wide env var, so parallel classes
/// would race. New test classes just derive this and belong to the same collection.
/// </summary>
[Collection(PlatformsCollection.Name)]
public abstract class PlatformsIntegrationTest(PlatformsApp app) : IntegrationTest<PlatformsApp>(app)
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new PlatformTypeJsonConverter() },
    };

    protected const PlatformType LinkedIn = PlatformType.LinkedIn;
    protected const PlatformType Facebook = PlatformType.Facebook;
    protected const PlatformType Instagram = PlatformType.Instagram;

    protected HttpClient CreateUserClient(Guid? userId = null) =>
        CreateClient(userId ?? Guid.NewGuid(), UserRoles.User);

    protected HttpClient CreateAdminClient(Guid? userId = null) => CreateClient(userId ?? Guid.NewGuid(), UserRoles.Admin);

    protected static async Task<List<PlatformResponse>> GetPlatformsAsync(HttpClient client)
    {
        var page = await client.GetFromJsonAsync<PagedResponse<PlatformResponse>>("/v1/platforms?pageSize=100", JsonOptions);
        return page!.Items;
    }

    protected static async Task<Guid> GetPlatformIdAsync(HttpClient client, PlatformType platformType) =>
        (await GetPlatformsAsync(client)).Single(p => p.PlatformType == platformType).Id;

    /// <summary>The add endpoint binds <c>[FromForm]</c>, so it takes a url-encoded form, not JSON.</summary>
    protected static Task<HttpResponseMessage> PostUserPlatformAsync(
        HttpClient client,
        Guid platformId,
        string accountUserName,
        string? accountComment = null)
    {
        var fields = new Dictionary<string, string>
        {
            ["PlatformId"] = platformId.ToString(),
            ["AccountUserName"] = accountUserName,
        };

        if (accountComment is not null)
        {
            fields["AccountComment"] = accountComment;
        }

        return client.PostAsync("/v1/user-platforms", new FormUrlEncodedContent(fields));
    }

    protected static async Task<Guid> AddUserPlatformAsync(
        HttpClient client,
        Guid platformId,
        string accountUserName,
        string? accountComment = null)
    {
        var response = await PostUserPlatformAsync(client, platformId, accountUserName, accountComment);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<UserPlatformIdResponse>(JsonOptions);
        return body!.UserPlatformId;
    }

    protected static async Task AssertErrorCodeAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.Should().Be(status);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        error!.Errors.Should().Contain(e => e.Code == code);
    }

    protected sealed record PagedResponse<T>(List<T> Items, int Page, int PageSize, long TotalCount);

    protected sealed record PlatformResponse(Guid Id, PlatformType PlatformType, bool IsOnline);

    protected sealed record UserPlatformIdResponse(Guid UserPlatformId);

    protected sealed record UserPlatformResponse(
        Guid Id,
        PlatformType PlatformType,
        string AccountUsername,
        DateTime CreatedAt,
        string? ExternalAccountId,
        string? AccountComment,
        string? ProfilePictureLink,
        Guid OwnerId);
}

[CollectionDefinition(PlatformsCollection.Name)]
public sealed class PlatformsCollection : ICollectionFixture<PlatformsApp>
{
    public const string Name = "PlatformsIntegration";
}
