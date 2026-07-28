using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Entities;

internal sealed class UserPlatform : BaseEntity, IAuditable
{
    public Guid UserId { get; private set; }
    public Guid PlatformId { get; private set; }
    public Platform Platform { get; private set; } = null!;
    public string? AccessToken { get; private set; }
    public string? ExternalAccountId { get; private set; }
    public string AccountUsername { get; private set; } = null!;
    public string? AccountComment { get; private set; }
    public string? ProfilePictureLink { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private UserPlatform()
    {

    }

    public static UserPlatform Create(Guid userId, Guid platformId, string accountUsername,
        string? accessToken, string? externalAccountId, string? accountComment, string? profilePictureLink)
    {
        return new UserPlatform
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlatformId = platformId,
            AccountUsername = accountUsername,
            AccessToken = accessToken,
            ExternalAccountId = externalAccountId,
            AccountComment = accountComment,
            ProfilePictureLink = profilePictureLink,
        };
    }

    public void UpdateToken(string accessToken) => AccessToken = accessToken;
}
