using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Tests.Integration.Platforms;

public sealed class PublishPostToLinkedInEndpointsTests(PlatformsApp app) : PlatformsIntegrationTest(app)
{
    [Fact]
    public async Task Publish_to_linkedin_calls_linkedin_and_records_publication()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var postId = App.Posts.Seed("Hello LinkedIn");
        var userPlatformId = await SeedLinkedInConnectionAsync(userId);

        var response = await client.PostAsJsonAsync(
            "/v1/linkedin/publish",
            new { postId, userPlatformId });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<PublishResponse>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.ExternalPostId.Should().Be("urn:li:share:text-1");

        App.LinkedIn.TextPosts.Should().ContainSingle(p => p.Content == "Hello LinkedIn");
        App.Posts.CreatedPublications.Should().ContainSingle(p =>
            p.PostId == postId
            && p.UserPlatformId == userPlatformId
            && p.ExternalPostId == "urn:li:share:text-1");
    }

    private async Task<Guid> SeedLinkedInConnectionAsync(Guid userId)
    {
        var platformId = await GetPlatformIdAsync(CreateUserClient(), PlatformType.LinkedIn);

        return await WithDbAsync<PlatformsWriteDbContext, Guid>(async db =>
        {
            var userPlatform = UserPlatform.Create(
                userId,
                platformId,
                "Jane Doe",
                "li-token",
                "sub-1",
                null,
                null);
            db.Set<UserPlatform>().Add(userPlatform);
            await db.SaveChangesAsync();
            return userPlatform.Id;
        });
    }

    private sealed record PublishResponse(Guid PostPublicationId, string ExternalPostId, DateTime PublishedAt);
}
